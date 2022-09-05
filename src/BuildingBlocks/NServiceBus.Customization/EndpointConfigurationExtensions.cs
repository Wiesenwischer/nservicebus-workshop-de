namespace NServiceBus;

public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration WithDefaults(this EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

        endpointConfiguration.EnableOutbox();

        endpointConfiguration.EnableInstallers();

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
