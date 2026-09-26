using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20260927_AddSoftDeleteQueryFiltersAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_statement_imports_account_hash_unique",
                table: "statement_imports");

            migrationBuilder.DropIndex(
                name: "ix_bank_transactions_account_fingerprint_unique",
                table: "bank_transactions");

            migrationBuilder.CreateIndex(
                name: "ix_statement_imports_account_hash_unique",
                table: "statement_imports",
                columns: new[] { "bank_account_id", "file_hash_sha256" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_account_fingerprint_unique",
                table: "bank_transactions",
                columns: new[] { "bank_account_id", "fingerprint" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_statement_imports_account_hash_unique",
                table: "statement_imports");

            migrationBuilder.DropIndex(
                name: "ix_bank_transactions_account_fingerprint_unique",
                table: "bank_transactions");

            migrationBuilder.CreateIndex(
                name: "ix_statement_imports_account_hash_unique",
                table: "statement_imports",
                columns: new[] { "bank_account_id", "file_hash_sha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_account_fingerprint_unique",
                table: "bank_transactions",
                columns: new[] { "bank_account_id", "fingerprint" },
                unique: true);
        }
    }
}
