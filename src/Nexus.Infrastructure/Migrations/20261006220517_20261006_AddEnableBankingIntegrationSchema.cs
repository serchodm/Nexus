using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20261006_AddEnableBankingIntegrationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bank_connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    institution_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    institution_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    external_session_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    encrypted_access_token = table.Column<string>(type: "text", nullable: true),
                    encrypted_refresh_token = table.Column<string>(type: "text", nullable: true),
                    consent_expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_bank_connections", x => x.id);
                    table.ForeignKey(
                        name: "fk_bank_connections_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "connected_bank_accounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_account_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    iban = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    account_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    available_balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    balance_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    internal_bank_account_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_connected_bank_accounts", x => x.id);
                    table.ForeignKey(
                        name: "fk_connected_bank_accounts_bank_accounts_internal_bank_account",
                        column: x => x.internal_bank_account_id,
                        principalTable: "bank_accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_connected_bank_accounts_bank_connections_bank_connection_id",
                        column: x => x.bank_connection_id,
                        principalTable: "bank_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bank_connections_external_session_id",
                table: "bank_connections",
                column: "external_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_connections_user_id_status",
                table: "bank_connections",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_connected_bank_accounts_bank_connection_id",
                table: "connected_bank_accounts",
                column: "bank_connection_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_bank_accounts_external_account_id",
                table: "connected_bank_accounts",
                column: "external_account_id");

            migrationBuilder.CreateIndex(
                name: "ix_connected_bank_accounts_internal_bank_account_id",
                table: "connected_bank_accounts",
                column: "internal_bank_account_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "connected_bank_accounts");

            migrationBuilder.DropTable(
                name: "bank_connections");
        }
    }
}
