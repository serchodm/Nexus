using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20261006_AddSharedExpensesAndLedgerSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shared_spaces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    default_split_rule = table.Column<int>(type: "integer", nullable: false),
                    default_owner_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    default_member_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_spaces", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shared_space_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    initials = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    avatar_color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    declared_monthly_income = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_space_members", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_space_members_shared_spaces_shared_space_id",
                        column: x => x.shared_space_id,
                        principalTable: "shared_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_space_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "shared_settlements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payer_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    receiver_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    method = table.Column<int>(type: "integer", nullable: false),
                    reference_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    settled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    linked_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_settlements", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_settlements_bank_transactions_linked_transaction_id",
                        column: x => x.linked_transaction_id,
                        principalTable: "bank_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_shared_settlements_shared_space_members_payer_member_id",
                        column: x => x.payer_member_id,
                        principalTable: "shared_space_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shared_settlements_shared_space_members_receiver_member_id",
                        column: x => x.receiver_member_id,
                        principalTable: "shared_space_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shared_settlements_shared_spaces_shared_space_id",
                        column: x => x.shared_space_id,
                        principalTable: "shared_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shared_expense_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    shared_space_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    transaction_split_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    paid_by_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    paid_by_share = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    owed_share = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    category_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category_icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    settlement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shared_expense_entries", x => x.id);
                    table.ForeignKey(
                        name: "fk_shared_expense_entries_bank_transactions_bank_transaction_id",
                        column: x => x.bank_transaction_id,
                        principalTable: "bank_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_shared_expense_entries_shared_settlements_settlement_id",
                        column: x => x.settlement_id,
                        principalTable: "shared_settlements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_shared_expense_entries_shared_space_members_paid_by_member_",
                        column: x => x.paid_by_member_id,
                        principalTable: "shared_space_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shared_expense_entries_shared_spaces_shared_space_id",
                        column: x => x.shared_space_id,
                        principalTable: "shared_spaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shared_expense_entries_transaction_split_items_transaction_",
                        column: x => x.transaction_split_item_id,
                        principalTable: "transaction_split_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shared_expense_entries_bank_tx_id",
                table: "shared_expense_entries",
                column: "bank_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_expense_entries_paid_by_member_id",
                table: "shared_expense_entries",
                column: "paid_by_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_expense_entries_settlement_id",
                table: "shared_expense_entries",
                column: "settlement_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_expense_entries_space_status",
                table: "shared_expense_entries",
                columns: new[] { "shared_space_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_shared_expense_entries_transaction_split_item_id",
                table: "shared_expense_entries",
                column: "transaction_split_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_settlements_linked_transaction_id",
                table: "shared_settlements",
                column: "linked_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_settlements_payer_member_id",
                table: "shared_settlements",
                column: "payer_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_settlements_receiver_member_id",
                table: "shared_settlements",
                column: "receiver_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_settlements_space_id",
                table: "shared_settlements",
                column: "shared_space_id");

            migrationBuilder.CreateIndex(
                name: "ix_shared_space_members_space_user",
                table: "shared_space_members",
                columns: new[] { "shared_space_id", "user_id" });

            migrationBuilder.CreateIndex(
                name: "ix_shared_space_members_user_id",
                table: "shared_space_members",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shared_expense_entries");

            migrationBuilder.DropTable(
                name: "shared_settlements");

            migrationBuilder.DropTable(
                name: "shared_space_members");

            migrationBuilder.DropTable(
                name: "shared_spaces");
        }
    }
}
