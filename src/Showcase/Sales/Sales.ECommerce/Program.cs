using Microsoft.AspNetCore.ResponseCompression;
using NServiceBus;
using Sales.ECommerce.Data;
using Sales.ECommerce.Hubs;
using Sales.Messages.Commands;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.MsSqlServer.Destructurers;

var configuration = GetConfiguration();
Log.Logger = CreateSerilogLogger(configuration, EndpointName);
ConfigureNServiceBusLogging(); 

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Host
    .UseSerilog()
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
            schemaName: ContextName,
            transport =>
            {
                // Here we configure our message routing
                var routing = transport.Routing();
                routing.RouteToEndpoint(typeof(PlaceOrder), "Ordering");

                transport.UseSchemaForEndpoint("Ordering", "Sales");
            });

    return endpointConfiguration;
});

var app = builder.Build();

app.UseResponseCompression();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapHub<ProductsHub>("/productsHub");
app.MapFallbackToPage("/_Host");

app.Run();

IConfiguration GetConfiguration()
{
    var configurationBuilder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    return configurationBuilder.Build();
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
            .WithDestructurers(new[] { new SqlExceptionDestructurer() })).Enrich.FromLogContext()
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

public partial class Program
{
    private const string ContextName = "Sales";
    private const string EndpointName = "ECommerce";
    private const string TransportConnectionStringName = "ServiceBus";
}
