namespace Sales.Messages.Events
{
    public class OrderPlaced
    {
        public OrderPlaced(string orderId, string clientId, string productId)
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
