using Microsoft.EntityFrameworkCore.Migrations;
using Stock.Inventory.Infrastructure;

#nullable disable

namespace Stock.Inventory.Migrations
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
