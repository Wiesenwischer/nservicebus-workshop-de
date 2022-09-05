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
    return Host.CreateDefaultBuilder(args)
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
                    Log.Fatal(msg, ex);
                    Log.CloseAndFlush();
                })
                .UseSqlServer(configuration.GetConnectionString(TransportConnectionStringName),
                    schemaName: ApplicationDbContext.DefaultSchema);

            return endpointConfiguration;
        })
        .UseSerilog()
        .ConfigureServices(services =>
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString(TransportConnectionStringName),
                        sqlServerOptionsAction: sqlOptions =>
                           {
                               sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                               sqlOptions.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
                           });
            });
        });
}

public partial class Program
{
    private const string EndpointName = "NServiceBusEndpoint";
    private const string TransportConnectionStringName = "ServiceBus";
}
