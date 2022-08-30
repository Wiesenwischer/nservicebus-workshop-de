using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBusEndpoint.Infrastructure;
using System;
using System.Threading.Tasks;

namespace NServiceBusEndpoint
{
    static class Program
    {
        private const string EndpointName = "NServiceBusEndpoint";

        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.MigrateDbContext<ApplicationDbContext>((_, __) => { });

            host.Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
            var connection = @"Data Source=sqldb;Database=Messaging;User ID=sa;Password=P@ssw0rd!#;Max Pool Size=100;Trust Server Certificate=True";

            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                })
                .UseNServiceBus(ctx =>
                {
                    // TODO: consider moving common endpoint configuration into a shared project
                    // for use by all endpoints in the system

                    var endpointConfiguration = new EndpointConfiguration(EndpointName);

                    endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

                    endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);

                    var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
                    transport.ConnectionString(connection);
                    transport.DefaultSchema("dbo");
                    transport.UseSchemaForQueue("error", "dbo");
                    transport.UseSchemaForQueue("audit", "dbo");

                    var subscriptions = transport.SubscriptionSettings();
                    subscriptions.DisableSubscriptionCache();

                    subscriptions.SubscriptionTableName(
                        tableName: "Subscriptions",
                        schemaName: "dbo");

                    var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
                    persistence.ConnectionBuilder(
                        connectionBuilder: () =>
                        {
                            return new SqlConnection(connection);
                        });
                    var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
                    dialect.Schema(ApplicationDbContext.DefaultSchema);
                    persistence.TablePrefix("");

                    endpointConfiguration.EnableOutbox();

                    endpointConfiguration.EnableInstallers();

                    return endpointConfiguration;
                })
                .ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                           {
                               options.UseSqlServer(connection,
                                   sqlServerOptionsAction: sqlOptions =>
                                   {
                                       sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                                       sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                                   });
                           });
                });
        }

        static async Task OnCriticalError(ICriticalErrorContext context)
        {
            try
            {
                await context.Stop();
            }
            finally
            {
                FailFast($"Critical error, shutting down: {context.Error}", context.Exception);
            }
        }

        static void FailFast(string message, Exception exception)
        {
            try
            {
                // TODO: decide what kind of last resort logging is necessary
                // TODO: when using an external logging framework it is important to flush any pending entries prior to calling FailFast
                // https://docs.particular.net/nservicebus/hosting/critical-errors#when-to-override-the-default-critical-error-action
            }
            finally
            {
                Environment.FailFast(message, exception);
            }
        }
    }
}