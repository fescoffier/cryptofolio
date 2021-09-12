using Microsoft.EntityFrameworkCore.Migrations;

namespace Cryptofolio.Infrastructure.Migrations
{
    public partial class AddAssetImagesUrl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "large_image_url",
                schema: "data",
                table: "asset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "small_image_url",
                schema: "data",
                table: "asset",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "thumb_image_url",
                schema: "data",
                table: "asset",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "large_image_url",
                schema: "data",
                table: "asset");

            migrationBuilder.DropColumn(
                name: "small_image_url",
                schema: "data",
                table: "asset");

            migrationBuilder.DropColumn(
                name: "thumb_image_url",
                schema: "data",
                table: "asset");
        }
    }
}
