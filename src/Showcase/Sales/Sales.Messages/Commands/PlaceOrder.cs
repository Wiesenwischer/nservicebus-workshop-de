namespace Sales.Messages.Commands
{
    public class PlaceOrder
    {
        public string? OrderId { get; set; }
        public string? ClientId { get; set; }
        public string? ProductId { get; set; }
    }
}
