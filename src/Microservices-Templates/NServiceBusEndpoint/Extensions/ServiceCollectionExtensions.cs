using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NServiceBusEndpoint.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, string connectionString)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy(), new[] { "liveness" });
            hcBuilder.AddCheck("servicebus", () => Program.ServiceBusState, new[] { "messaging", "nservicebus" });

            hcBuilder
                .AddSqlServer(connectionString,
                    name: "database",
                    tags: new [] { "data", "sqldb" });

            return services;
        }
    }
}
