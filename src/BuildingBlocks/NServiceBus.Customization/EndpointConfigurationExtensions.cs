namespace NServiceBus;

public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration WithDefaults(this EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseSerialization<NewtonsoftJsonSerializer>();

        endpointConfiguration.EnableOutbox();

        endpointConfiguration.EnableInstallers();

        var conventions = endpointConfiguration.Conventions();
        conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
        conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));
        conventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith("Messages"));

        return endpointConfiguration;
    }

    public static EndpointConfiguration FailFastOnCriticalError(this EndpointConfiguration endpointConfiguration, Action<string, Exception> callback)
    {
        endpointConfiguration.DefineCriticalErrorAction(ctx => OnCriticalError(ctx, callback));

        return endpointConfiguration;
    }

    private static async Task OnCriticalError(ICriticalErrorContext context, Action<string, Exception> callback)
    {
        try
        {
            await context.Stop();
        }
        finally
        {
            FailFast($"Critical error, shutting down: {context.Error}", context.Exception, callback);
        }
    }

    private static void FailFast(string message, Exception exception, Action<string, Exception> callback)
    {
        try
        {
            callback?.Invoke(message, exception);
        }
        finally
        {
            Environment.FailFast(message, exception);
        }
    }
}
