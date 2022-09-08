namespace Stock.Messages.Events
{
    public class OrderStockConfirmed
    {
        public OrderStockConfirmed(string orderId)
        {
            OrderId = orderId;
        }

        public string OrderId { get; set; }
    }
}
