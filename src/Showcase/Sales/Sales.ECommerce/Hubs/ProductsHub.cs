using Microsoft.AspNetCore.SignalR;
using Sales.ECommerce.Data;

namespace Sales.ECommerce.Hubs
{
    public class ProductsHub : Hub
    {
        public Task PlaceOrder(Product product)
        {
            throw new NotImplementedException();
        }
    }
}
