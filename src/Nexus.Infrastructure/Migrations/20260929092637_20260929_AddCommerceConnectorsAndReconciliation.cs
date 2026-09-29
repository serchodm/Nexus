using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20260929_AddCommerceConnectorsAndReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "external_commerce_connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    account_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    encrypted_access_token = table.Column<string>(type: "text", nullable: false),
                    encrypted_refresh_token = table.Column<string>(type: "text", nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sync_status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_commerce_connections", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_commerce_connections_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "external_commerce_orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    external_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    shipment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    counterparty_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    raw_data_json = table.Column<string>(type: "jsonb", nullable: true),
                    is_fully_reconciled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_commerce_orders", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_commerce_orders_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "external_commerce_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_commerce_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    category_hint = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    condition = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    bank_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_external_commerce_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_external_commerce_order_items_bank_transactions_bank_transa",
                        column: x => x.bank_transaction_id,
                        principalTable: "bank_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_external_commerce_order_items_external_commerce_orders_exte",
                        column: x => x.external_commerce_order_id,
                        principalTable: "external_commerce_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_reconciliations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_commerce_order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    confidence = table.Column<int>(type: "integer", nullable: false),
                    match_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    matched_order_item_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false),
                    reconciled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    applied_as_splits = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_reconciliations", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_reconciliations_bank_transactions_bank_transact",
                        column: x => x.bank_transaction_id,
                        principalTable: "bank_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_reconciliations_external_commerce_orders_extern",
                        column: x => x.external_commerce_order_id,
                        principalTable: "external_commerce_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_reconciliations_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_commerce_connections_user_provider",
                table: "external_commerce_connections",
                columns: new[] { "user_id", "provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_commerce_items_order_id",
                table: "external_commerce_order_items",
                column: "external_commerce_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_commerce_items_tx_id",
                table: "external_commerce_order_items",
                column: "bank_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_external_commerce_orders_unique",
                table: "external_commerce_orders",
                columns: new[] { "user_id", "provider", "external_order_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_external_commerce_orders_user_date",
                table: "external_commerce_orders",
                columns: new[] { "user_id", "order_date" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliations_tx_order",
                table: "transaction_reconciliations",
                columns: new[] { "bank_transaction_id", "external_commerce_order_id" });

            migrationBuilder.CreateIndex(
                name: "ix_reconciliations_user_status",
                table: "transaction_reconciliations",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_reconciliations_external_commerce_order_id",
                table: "transaction_reconciliations",
                column: "external_commerce_order_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "external_commerce_connections");

            migrationBuilder.DropTable(
                name: "external_commerce_order_items");

            migrationBuilder.DropTable(
                name: "transaction_reconciliations");

            migrationBuilder.DropTable(
                name: "external_commerce_orders");
        }
    }
}
