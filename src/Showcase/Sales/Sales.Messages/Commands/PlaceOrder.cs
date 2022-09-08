namespace Sales.Messages.Commands
{
    public class PlaceOrder
    {
        public PlaceOrder(string orderId, string clientId, string productId)
        {
            OrderId = orderId;
            ClientId = clientId;
            ProductId = productId;
        }

        public string OrderId { get; }
        public string ClientId { get; }
        public string ProductId { get; }
    }
}
