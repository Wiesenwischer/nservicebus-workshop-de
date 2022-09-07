using Sales.Messages.Commands;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sales.Ordering.Application.Handlers;

public class PlaceOrderHandler :
    IHandleMessages<PlaceOrder>
{
    private readonly ILogger _logger;

    public PlaceOrderHandler(ILogger<PlaceOrderHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task Handle(PlaceOrder message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Received PlaceOrder: {@Message}", message);
        return Task.CompletedTask;
    }
}

