namespace Sales.Messages.Events
{
    public class OrderPlaced
    {
        public string? OrderId { get; set; }
        public string? ClientId { get; set; }
        public string? ProductId { get; set; }
    }
}
