namespace NServiceBusEndpoint.Infrastructure;

public class ApplicationDbContextDesignFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=.;Initial Catalog=Messaging;Integrated Security=True");

        return new ApplicationDbContext(optionsBuilder.Options);
    }       
}
