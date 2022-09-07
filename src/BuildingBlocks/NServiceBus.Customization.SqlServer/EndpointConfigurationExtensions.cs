namespace NServiceBus;

public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration UseSqlServer(this EndpointConfiguration endpointConfiguration,
        string connectionString, string schemaName, Action<TransportExtensions<SqlServerTransport>> configureTransport)
    {
        var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
        transport.ConnectionString(connectionString);
        transport.DefaultSchema(schemaName);
        transport.UseSchemaForQueue("error", "dbo");
        transport.UseSchemaForQueue("audit", "dbo");

        var subscriptions = transport.SubscriptionSettings();
        subscriptions.DisableSubscriptionCache();

        subscriptions.SubscriptionTableName(
            tableName: "Subscriptions",
            schemaName: "dbo");

        var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
        persistence.ConnectionBuilder(
            connectionBuilder: () => new SqlConnection(connectionString));
        var dialect = persistence.SqlDialect<SqlDialect.MsSqlServer>();
        dialect.Schema(schemaName);
        persistence.TablePrefix("");

        configureTransport?.Invoke(transport);

        return endpointConfiguration;
    }

    public static EndpointConfiguration UseSqlServer(this EndpointConfiguration endpointConfiguration,
        string connectionString, string schemaName, Action<RoutingSettings<SqlServerTransport>> configureRouting)
    {
        return endpointConfiguration.UseSqlServer(connectionString, schemaName, configureRouting);
    }
}