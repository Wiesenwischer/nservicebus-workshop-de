using Sales.Messages.Events;
using Stock.Messages.Events;
using System.Collections.Generic;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Stock.Inventory.Application.Handlers;

public class OrderPlacedHandler
    : IHandleMessages<OrderPlaced>
{
    private readonly ILogger _logger;

    public OrderPlacedHandler(ILogger<OrderPlacedHandler> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(OrderPlaced message, IMessageHandlerContext context)
    {
        _logger.LogInformation("Received OrderPlaced: {@Message}", message);

        if (_availableProductsList.Contains(message.ProductId))
        {
            await context.Publish(new OrderStockConfirmed(message.OrderId));
            _logger.LogInformation("Published {MessageType} for OrderId: {OrderId}",nameof(OrderStockConfirmed), message.OrderId);
        }
        else
        {
            await context.Publish(new OrderStockRejected(message.OrderId));
            _logger.LogInformation("Published {MessageType} for OrderId: {OrderId}", nameof(OrderStockRejected), message.OrderId);
        }
    }

    private readonly List<string> _availableProductsList = new()
        {
            "videos", "documentation", "platform"
        };
}

