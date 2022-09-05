namespace NServiceBus;

public class EndpointConfigurationBuilder
{
    public static EndpointConfiguration Configure(string endpointName)
    {
        var endpointConfiguration = new EndpointConfiguration(endpointName);

        return endpointConfiguration;
    }
}