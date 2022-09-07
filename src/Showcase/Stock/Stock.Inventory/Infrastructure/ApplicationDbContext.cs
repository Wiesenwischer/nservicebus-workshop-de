namespace Stock.Inventory.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public const string DefaultSchema = "Stock";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
    }
}