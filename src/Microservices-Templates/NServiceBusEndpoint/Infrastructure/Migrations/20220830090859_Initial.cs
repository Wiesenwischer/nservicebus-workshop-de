using Microsoft.EntityFrameworkCore.Migrations;
using NServiceBusEndpoint.Infrastructure;

#nullable disable

namespace NServiceBusEndpoint.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: ApplicationDbContext.DefaultSchema);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
