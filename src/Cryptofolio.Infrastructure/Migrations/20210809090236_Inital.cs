using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Cryptofolio.Infrastructure.Migrations
{
    public partial class Inital : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "data");

            migrationBuilder.CreateTable(
                name: "asset",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    symbol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exchange",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    year_established = table.Column<long>(type: "bigint", nullable: true),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    image = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchange", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "setting",
                schema: "data",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    group = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_setting", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    selected = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_ticker",
                schema: "data",
                columns: table => new
                {
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    vs_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    asset_id = table.Column<string>(type: "character varying(100)", nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_ticker", x => new { x.asset_id, x.timestamp, x.vs_currency });
                    table.ForeignKey(
                        name: "FK_asset_ticker_asset_asset_id",
                        column: x => x.asset_id,
                        principalSchema: "data",
                        principalTable: "asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asset_ticker_timestamp",
                schema: "data",
                table: "asset_ticker",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_asset_ticker_vs_currency",
                schema: "data",
                table: "asset_ticker",
                column: "vs_currency");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_ticker",
                schema: "data");

            migrationBuilder.DropTable(
                name: "exchange",
                schema: "data");

            migrationBuilder.DropTable(
                name: "setting",
                schema: "data");

            migrationBuilder.DropTable(
                name: "wallet",
                schema: "data");

            migrationBuilder.DropTable(
                name: "asset",
                schema: "data");
        }
    }
}
