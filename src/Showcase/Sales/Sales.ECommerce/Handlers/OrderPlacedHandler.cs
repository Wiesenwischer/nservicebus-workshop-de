using Microsoft.AspNetCore.SignalR;
using NServiceBus;
using Sales.ECommerce.Hubs;
using Sales.Messages.Events;

namespace Sales.ECommerce.Handlers
{
    public class OrderPlacedHandler :
        IHandleMessages<OrderPlaced>
    {
        private IHubContext<ProductsHub> ordersHubContext;

        public OrderPlacedHandler(IHubContext<ProductsHub> ordersHubContext)
        {
            this.ordersHubContext = ordersHubContext;
        }

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            return ordersHubContext.Clients.Client(message.ClientId).SendAsync("OrderReceived", message.OrderId);
        }
    }
}
