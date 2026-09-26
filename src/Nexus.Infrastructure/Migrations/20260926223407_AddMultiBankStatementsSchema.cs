using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiBankStatementsSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    bank_type = table.Column<int>(type: "integer", nullable: false),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bank_code = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: true),
                    iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    last4digits = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    type = table.Column<int>(type: "integer", nullable: false),
                    holder_role = table.Column<int>(type: "integer", nullable: false),
                    ownership_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 100.00m),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    balance_last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    linked_cards_last4 = table.Column<List<string>>(type: "text[]", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "statement_imports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    file_hash_sha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    detected_bank_type = table.Column<int>(type: "integer", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    total_records = table.Column<int>(type: "integer", nullable: false),
                    imported_records = table.Column<int>(type: "integer", nullable: false),
                    skipped_duplicates = table.Column<int>(type: "integer", nullable: false),
                    starting_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ending_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_statement_imports", x => x.id);
                    table.ForeignKey(
                        name: "fk_statement_imports_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bank_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    statement_import_id = table.Column<Guid>(type: "uuid", nullable: false),
                    operation_date = table.Column<DateOnly>(type: "date", nullable: false),
                    value_date = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    balance_after = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    bank_category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_subcategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_movement_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    observations = table.Column<string>(type: "text", nullable: true),
                    raw_data_json = table.Column<string>(type: "jsonb", nullable: false),
                    fingerprint = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_transactions_bank_accounts_bank_account_id",
                        column: x => x.bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_bank_transactions_statement_imports_statement_import_id",
                        column: x => x.statement_import_id,
                        principalTable: "statement_imports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_user_iban_unique",
                table: "bank_accounts",
                columns: new[] { "user_id", "iban" },
                unique: true,
                filter: "is_deleted = false AND iban IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_bank_accounts_user_id",
                table: "bank_accounts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_account_date",
                table: "bank_transactions",
                columns: new[] { "bank_account_id", "operation_date" });

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_account_fingerprint_unique",
                table: "bank_transactions",
                columns: new[] { "bank_account_id", "fingerprint" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_statement_import_id",
                table: "bank_transactions",
                column: "statement_import_id");

            migrationBuilder.CreateIndex(
                name: "ix_statement_imports_account_hash_unique",
                table: "statement_imports",
                columns: new[] { "bank_account_id", "file_hash_sha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_statement_imports_bank_account_id",
                table: "statement_imports",
                column: "bank_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bank_transactions");

            migrationBuilder.DropTable(
                name: "statement_imports");

            migrationBuilder.DropTable(
                name: "bank_accounts");
        }
    }
}
