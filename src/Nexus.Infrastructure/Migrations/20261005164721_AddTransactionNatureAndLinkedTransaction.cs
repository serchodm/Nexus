using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionNatureAndLinkedTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "linked_transaction_id",
                table: "bank_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "transaction_nature",
                table: "bank_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_linked_transaction_id",
                table: "bank_transactions",
                column: "linked_transaction_id");

            migrationBuilder.AddForeignKey(
                name: "fk_bank_transactions_bank_transactions_linked_transaction_id",
                table: "bank_transactions",
                column: "linked_transaction_id",
                principalTable: "bank_transactions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bank_transactions_bank_transactions_linked_transaction_id",
                table: "bank_transactions");

            migrationBuilder.DropIndex(
                name: "ix_bank_transactions_linked_transaction_id",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "linked_transaction_id",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "transaction_nature",
                table: "bank_transactions");
        }
    }
}
