namespace Sales.ECommerce.Data
{
    public class Product
    {
        public Product(string identifier, string title, string description)
        {
            Identifier = identifier;
            Title = title;
            Description = description;
        }

        public string Identifier { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
