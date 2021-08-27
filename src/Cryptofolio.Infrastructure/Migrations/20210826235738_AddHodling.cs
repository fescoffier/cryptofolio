using Microsoft.EntityFrameworkCore.Migrations;

namespace Cryptofolio.Infrastructure.Migrations
{
    public partial class AddHodling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "holding",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    wallet_id = table.Column<string>(type: "character varying(36)", nullable: false),
                    asset_id = table.Column<string>(type: "character varying(100)", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holding", x => x.id);
                    table.ForeignKey(
                        name: "FK_holding_asset_asset_id",
                        column: x => x.asset_id,
                        principalSchema: "data",
                        principalTable: "asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_holding_wallet_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "data",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_holding_asset_id",
                schema: "data",
                table: "holding",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_holding_wallet_id_asset_id",
                schema: "data",
                table: "holding",
                columns: new[] { "wallet_id", "asset_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "holding",
                schema: "data");
        }
    }
}
