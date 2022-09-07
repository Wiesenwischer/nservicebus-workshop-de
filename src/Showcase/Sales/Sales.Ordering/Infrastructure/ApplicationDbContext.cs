namespace Sales.Ordering.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public const string DefaultSchema = "Sales";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
    }
}