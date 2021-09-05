using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Cryptofolio.Infrastructure.Migrations
{
    public partial class Initial : Migration
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
                name: "currency",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    symbol = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    precision = table.Column<int>(type: "integer", nullable: false, defaultValue: 2)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_currency", x => x.id);
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
                name: "asset_ticker",
                schema: "data",
                columns: table => new
                {
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    asset_id = table.Column<string>(type: "character varying(100)", nullable: false),
                    vs_currency_id = table.Column<string>(type: "character varying(36)", nullable: false),
                    value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_ticker", x => new { x.asset_id, x.vs_currency_id, x.timestamp });
                    table.ForeignKey(
                        name: "FK_asset_ticker_asset_asset_id",
                        column: x => x.asset_id,
                        principalSchema: "data",
                        principalTable: "asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_asset_ticker_currency_vs_currency_id",
                        column: x => x.vs_currency_id,
                        principalSchema: "data",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "wallet",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    currency_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    InitialValue = table.Column<decimal>(type: "numeric", nullable: false),
                    current_value = table.Column<decimal>(type: "numeric", nullable: false),
                    change = table.Column<decimal>(type: "numeric", nullable: false),
                    selected = table.Column<bool>(type: "boolean", nullable: false),
                    user_id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet", x => x.id);
                    table.ForeignKey(
                        name: "FK_wallet_currency_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "data",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "holding",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    wallet_id = table.Column<string>(type: "character varying(36)", nullable: false),
                    asset_id = table.Column<string>(type: "character varying(100)", nullable: false),
                    qty = table.Column<decimal>(type: "numeric", nullable: false),
                    InitialValue = table.Column<decimal>(type: "numeric", nullable: false),
                    current_value = table.Column<decimal>(type: "numeric", nullable: false),
                    change = table.Column<decimal>(type: "numeric", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "data",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    wallet_id = table.Column<string>(type: "character varying(36)", nullable: false),
                    asset_id = table.Column<string>(type: "character varying(100)", nullable: false),
                    exchange_id = table.Column<string>(type: "character varying(100)", nullable: true),
                    qty = table.Column<decimal>(type: "numeric", nullable: false),
                    current_value = table.Column<decimal>(type: "numeric", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    currency_id = table.Column<string>(type: "character varying(36)", nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    initial_value = table.Column<decimal>(type: "numeric", nullable: true),
                    change = table.Column<decimal>(type: "numeric", nullable: true),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    destination = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_asset_asset_id",
                        column: x => x.asset_id,
                        principalSchema: "data",
                        principalTable: "asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_currency_currency_id",
                        column: x => x.currency_id,
                        principalSchema: "data",
                        principalTable: "currency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_transaction_exchange_exchange_id",
                        column: x => x.exchange_id,
                        principalSchema: "data",
                        principalTable: "exchange",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_wallet_wallet_id",
                        column: x => x.wallet_id,
                        principalSchema: "data",
                        principalTable: "wallet",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asset_symbol",
                schema: "data",
                table: "asset",
                column: "symbol");

            migrationBuilder.CreateIndex(
                name: "IX_asset_ticker_timestamp",
                schema: "data",
                table: "asset_ticker",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_asset_ticker_vs_currency_id",
                schema: "data",
                table: "asset_ticker",
                column: "vs_currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_currency_code",
                schema: "data",
                table: "currency",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_currency_ticker_timestamp",
                schema: "data",
                table: "currency_ticker",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_currency_ticker_vs_currency_id",
                schema: "data",
                table: "currency_ticker",
                column: "vs_currency_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_setting_group",
                schema: "data",
                table: "setting",
                column: "group");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_asset_id",
                schema: "data",
                table: "transaction",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_currency_id",
                schema: "data",
                table: "transaction",
                column: "currency_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_date",
                schema: "data",
                table: "transaction",
                column: "date")
                .Annotation("Npgsql:IndexSortOrder", new[] { SortOrder.Descending });

            migrationBuilder.CreateIndex(
                name: "IX_transaction_destination",
                schema: "data",
                table: "transaction",
                column: "destination");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_exchange_id",
                schema: "data",
                table: "transaction",
                column: "exchange_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_source",
                schema: "data",
                table: "transaction",
                column: "source");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_type",
                schema: "data",
                table: "transaction",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_wallet_id",
                schema: "data",
                table: "transaction",
                column: "wallet_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_currency_id",
                schema: "data",
                table: "wallet",
                column: "currency_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asset_ticker",
                schema: "data");

            migrationBuilder.DropTable(
                name: "currency_ticker",
                schema: "data");

            migrationBuilder.DropTable(
                name: "holding",
                schema: "data");

            migrationBuilder.DropTable(
                name: "setting",
                schema: "data");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "data");

            migrationBuilder.DropTable(
                name: "asset",
                schema: "data");

            migrationBuilder.DropTable(
                name: "exchange",
                schema: "data");

            migrationBuilder.DropTable(
                name: "wallet",
                schema: "data");

            migrationBuilder.DropTable(
                name: "currency",
                schema: "data");
        }
    }
}
