using Microsoft.EntityFrameworkCore.Migrations;

namespace Cryptofolio.Infrastructure.Migrations
{
    public partial class AddCurrencyValueFormat : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "value_format",
                schema: "data",
                table: "currency",
                type: "text",
                nullable: false,
                defaultValue: "{0}{1}");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "value_format",
                schema: "data",
                table: "currency");
        }
    }
}
