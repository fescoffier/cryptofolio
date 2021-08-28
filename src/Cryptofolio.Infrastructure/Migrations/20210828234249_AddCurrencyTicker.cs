using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Cryptofolio.Infrastructure.Migrations
{
    public partial class AddCurrencyTicker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "currency_ticker",
                schema: "data",
                columns: table => new
                {
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    currency_id = table.Column<string>(type: "character varying(36)", nullable: false),
                    vs_currency_id = table.Column<string>(type: "character varying(36)", nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency_ticker", x => new { x.currency_id, x.vs_currency_id, x.timestamp });
                    table.ForeignKey(
                        name: "FK_currency_ticker_currency_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "data",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_currency_ticker_currency_vs_currency_id",
                        column: x => x.vs_currency_id,
                        principalSchema: "data",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_currency_ticker_vs_currency_id",
                schema: "data",
                table: "currency_ticker",
                column: "vs_currency_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "currency_ticker",
                schema: "data");
        }
    }
}
