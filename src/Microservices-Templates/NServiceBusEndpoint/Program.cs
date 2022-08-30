using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NServiceBusEndpoint
{
    static class Program
    {
        private const string EndpointName = "NServiceBusEndpoint";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args)
        {
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

                    // TODO: remove this condition after choosing a transport, persistence and deployment method suitable for production
                    if (Environment.UserInteractive && Debugger.IsAttached)
                    {
                        var connection = @"Data Source=sqldb;Database=Messaging;User ID=sa;Password=P@ssw0rd!#;Max Pool Size=100";

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
                        persistence.TablePrefix("");

                        endpointConfiguration.EnableOutbox();

                        // TODO: create a script for deployment to production
                        endpointConfiguration.EnableInstallers();
                    }

                    // TODO: replace the license.xml file with your license file

                    return endpointConfiguration;
                });
        }

        static async Task OnCriticalError(ICriticalErrorContext context)
        {
            // TODO: decide if stopping the endpoint and exiting the process is the best response to a critical error
            // https://docs.particular.net/nservicebus/hosting/critical-errors
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