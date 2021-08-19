﻿using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace Cryptofolio.Infrastructure.Migrations
{
    public partial class AddTransactionEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    note = table.Column<string>(type: "text", nullable: true),
                    discriminator = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
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
                name: "IX_transaction_asset_id",
                schema: "data",
                table: "transaction",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_currency",
                schema: "data",
                table: "transaction",
                column: "currency");

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transaction",
                schema: "data");
        }
    }
}
