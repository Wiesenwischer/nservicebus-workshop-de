var configuration = GetConfiguration();
Log.Logger = CreateSerilogLogger(configuration, EndpointName);
ConfigureNServiceBusLogging();

try
{
    Log.Information("Configuring web host ({ApplicationContext})...", EndpointName);
    var host = CreateHostBuilder(args, configuration)
        .Build();

    host.MigrateDbContext<ApplicationDbContext>((_, __) => { });

    Log.Information("Starting web host ({ApplicationContext})...", EndpointName);
    host.Run();

    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", EndpointName);
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}

Serilog.ILogger CreateSerilogLogger(IConfiguration configuration, string applicationContext)
{
    string logstashUrl = configuration["Serilog:LogstashUrl"];

    return new Serilog.LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", applicationContext)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

void ConfigureNServiceBusLogging()
{
    var extensionsLoggerFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory();
    var nservicebusLoggerFactory = new NServiceBus.Extensions.Logging.ExtensionsLoggerFactory(extensionsLoggerFactory);
    NServiceBus.Logging.LogManager.UseFactory(nservicebusLoggerFactory);
}

IHostBuilder CreateHostBuilder(string[] args, IConfiguration configuration)
{
    var connection = @"Data Source=sqldb;Database=Messaging;User ID=sa;Password=P@ssw0rd!#;Max Pool Size=100;Trust Server Certificate=True";

    return Host.CreateDefaultBuilder(args)
        .UseConsoleLifetime()
        .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
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
        .UseSerilog()
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

async Task OnCriticalError(ICriticalErrorContext context)
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

void FailFast(string message, Exception exception)
{
    try
    {
        Log.Fatal(message, exception);
        Log.CloseAndFlush();
    }
    finally
    {
        Environment.FailFast(message, exception);
    }
}

public partial class Program
{
    private const string EndpointName = "NServiceBusEndpoint";
}
