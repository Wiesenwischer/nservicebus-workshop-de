namespace Sales.Ordering.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public const string DefaultSchema = "Sales.Ordering";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
    }
}