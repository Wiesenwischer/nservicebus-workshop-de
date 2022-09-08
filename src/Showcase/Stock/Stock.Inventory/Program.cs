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

    ServiceBusState = HealthCheckResult.Healthy();

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
        .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
            .WithDefaultDestructurers()
            .WithDestructurers(new[] { new SqlExceptionDestructurer() }))
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
    return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(builder =>
            {
                builder.UseStartup<Startup>();
            })
            .UseConsoleLifetime()
            .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            })
            .UseNServiceBus(ctx =>
            {
                var endpointConfiguration = EndpointConfigurationBuilder.Configure(EndpointName)
                    .WithDefaults()
                    .FailFastOnCriticalError((msg, ex) =>
                    {
                        ServiceBusState = HealthCheckResult.Unhealthy("Critical error on endpoint", ex);
                        Log.Fatal(msg, ex);
                        Log.CloseAndFlush();
                    })
                    .UseSqlServer(configuration.GetConnectionString(TransportConnectionStringName),
                        schemaName: ApplicationDbContext.DefaultSchema,
                        transport =>
                        {
                            transport.TimeToWaitBeforeTriggeringCircuitBreaker(
                                TimeSpan.FromMinutes(configuration.GetValue("ServiceBusTimeToWaitBeforeTriggeringCircuitBreaker",
                                    DefaultTimeInMinutesToWaitBeforeTriggeringCircuitBreaker)));
                            
                            // Here we configure our message routing
                            // var routing = transport.Routing();
                            // routing.RouteToEndpoint(typeof(MyMessage), "DestinationEndpointName");
                            // routing.RouteToEndpoint(typeof(MyMessage).Assembly, "DestinationEndpointName")

                            // Here we configure the schema mappings
                            // transport.UseSchemaForEndpoint("MyEndpoint1", "Schema1");
                        });

                return endpointConfiguration;
            })
            .UseSerilog();
}

public partial class Program
{
    public static HealthCheckResult ServiceBusState { get; private set; }
    private const int DefaultTimeInMinutesToWaitBeforeTriggeringCircuitBreaker = 120;
    private const string EndpointName = "Inventory";
    public const string TransportConnectionStringName = "ServiceBus";
}
