using Microsoft.AspNetCore.SignalR;
using NServiceBus;
using Sales.ECommerce.Data;
using Sales.Messages.Commands;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Sales.ECommerce.Hubs
{
    public class ProductsHub : Hub
    {
        private readonly IMessageSession _messageSession;
        private readonly ILogger _logger;

        public ProductsHub(IMessageSession messageSession, ILogger<ProductsHub> logger)
        {
            _messageSession = messageSession ?? throw new ArgumentNullException(nameof(messageSession));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task PlaceOrder(Product product)
        {
            var placeOrder = new PlaceOrder
            {
                OrderId = Guid.NewGuid().ToString(),
                ClientId = this.Context.ConnectionId,
                ProductId = product.Identifier
            };

            try
            {
                await _messageSession.Send(placeOrder);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error sending command '{CommandName}'", nameof(PlaceOrder));
                throw;
            }
        }
    }
}
