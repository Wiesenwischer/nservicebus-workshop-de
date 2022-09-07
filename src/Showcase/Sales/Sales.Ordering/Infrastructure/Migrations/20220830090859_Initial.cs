using Microsoft.EntityFrameworkCore.Migrations;
using Sales.Ordering.Infrastructure;

#nullable disable

namespace Sales.Ordering.Migrations
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
