namespace Stock.Messages.Events
{
    public class OrderStockRejected
    {
        public OrderStockRejected(string orderId)
        {
            OrderId = orderId;
        }

        public string OrderId { get; }
    }
}

