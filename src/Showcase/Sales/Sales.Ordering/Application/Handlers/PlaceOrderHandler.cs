using Sales.Messages.Commands;
using Sales.Messages.Events;
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

        var orderPlaced = new OrderPlaced
        {
            OrderId = message.OrderId,
            ClientId = message.ClientId,
            ProductId = message.ProductId
        };
        return context.Publish(orderPlaced);
    }
}

