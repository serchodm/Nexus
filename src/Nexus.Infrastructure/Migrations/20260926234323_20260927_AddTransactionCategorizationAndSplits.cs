using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20260927_AddTransactionCategorizationAndSplits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "categorization_source",
                table: "bank_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "bank_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "clean_merchant_name",
                table: "bank_transactions",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "expense_flexibility",
                table: "bank_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "expense_nature",
                table: "bank_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "external_platform",
                table: "bank_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "has_splits",
                table: "bank_transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_manual",
                table: "bank_transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_recurring",
                table: "bank_transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "pending_platform_enrichment",
                table: "bank_transactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "recurrence_period",
                table: "bank_transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "subcategory_id",
                table: "bank_transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    normalized_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "tag"),
                    color_hex = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "#64748B"),
                    flow_type = table.Column<int>(type: "integer", nullable: false),
                    default_expense_nature = table.Column<int>(type: "integer", nullable: false),
                    is_system = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_categories_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_categories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "categorization_rules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    pattern = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    normalized_pattern = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    match_type = table.Column<int>(type: "integer", nullable: false),
                    target_category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_subcategory_id = table.Column<Guid>(type: "uuid", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    set_expense_nature = table.Column<int>(type: "integer", nullable: true),
                    set_is_recurring = table.Column<bool>(type: "boolean", nullable: true),
                    set_expense_flexibility = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categorization_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_categorization_rules_categories_target_category_id",
                        column: x => x.target_category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_categorization_rules_categories_target_subcategory_id",
                        column: x => x.target_subcategory_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_categorization_rules_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_split_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subcategory_id = table.Column<Guid>(type: "uuid", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    expense_nature = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transaction_split_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_transaction_split_items_bank_transactions_bank_transaction_",
                        column: x => x.bank_transaction_id,
                        principalTable: "bank_transactions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transaction_split_items_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_transaction_split_items_categories_subcategory_id",
                        column: x => x.subcategory_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "color_hex", "created_at", "default_expense_nature", "deleted_at", "display_order", "flow_type", "icon", "is_deleted", "is_system", "name", "normalized_name", "parent_category_id", "updated_at", "user_id" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0001-000000000000"), "#10B981", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 1, 1, "utensils", false, true, "Alimentación", "ALIMENTACION", null, null, null },
                    { new Guid("00000000-0000-0000-0002-000000000000"), "#3B82F6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 2, 1, "home", false, true, "Hogar y Vivienda", "HOGAR Y VIVIENDA", null, null, null },
                    { new Guid("00000000-0000-0000-0003-000000000000"), "#F59E0B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 1, "car", false, true, "Transporte", "TRANSPORTE", null, null, null },
                    { new Guid("00000000-0000-0000-0004-000000000000"), "#8B5CF6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 4, 1, "tv", false, true, "Suscripciones y Servicios", "SUSCRIPCIONES Y SERVICIOS", null, null, null },
                    { new Guid("00000000-0000-0000-0005-000000000000"), "#EC4899", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 5, 1, "sparkles", false, true, "Ocio y Estilo de Vida", "OCIO Y ESTILO DE VIDA", null, null, null },
                    { new Guid("00000000-0000-0000-0006-000000000000"), "#06B6D4", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 6, 1, "heart-pulse", false, true, "Salud y Bienestar", "SALUD Y BIENESTAR", null, null, null },
                    { new Guid("00000000-0000-0000-0007-000000000000"), "#22C55E", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 7, 2, "wallet", false, true, "Finanzas e Ingresos", "FINANZAS E INGRESOS", null, null, null },
                    { new Guid("00000000-0000-0000-0008-000000000000"), "#64748B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 8, 1, "receipt", false, true, "Impuestos y Tasas", "IMPUESTOS Y TASAS", null, null, null },
                    { new Guid("00000000-0000-0000-0001-000000000001"), "#10B981", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 1, 1, "shopping-cart", false, true, "Supermercados", "SUPERMERCADOS", new Guid("00000000-0000-0000-0001-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0001-000000000002"), "#10B981", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 2, 1, "utensils", false, true, "Restaurantes y Bares", "RESTAURANTES Y BARES", new Guid("00000000-0000-0000-0001-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0001-000000000003"), "#10B981", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 1, "coffee", false, true, "Cafeterías", "CAFETERIAS", new Guid("00000000-0000-0000-0001-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0001-000000000004"), "#10B981", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 4, 1, "bike", false, true, "Comida a Domicilio", "COMIDA A DOMICILIO", new Guid("00000000-0000-0000-0001-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0002-000000000001"), "#3B82F6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 1, 1, "key", false, true, "Alquiler e Hipoteca", "ALQUILER E HIPOTECA", new Guid("00000000-0000-0000-0002-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0002-000000000002"), "#3B82F6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 2, 1, "zap", false, true, "Electricidad y Gas", "ELECTRICIDAD Y GAS", new Guid("00000000-0000-0000-0002-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0002-000000000003"), "#3B82F6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 3, 1, "droplet", false, true, "Agua", "AGUA", new Guid("00000000-0000-0000-0002-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0002-000000000004"), "#3B82F6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 4, 1, "wifi", false, true, "Internet y Teléfono", "INTERNET Y TELEFONO", new Guid("00000000-0000-0000-0002-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0002-000000000005"), "#3B82F6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2, null, 5, 1, "hammer", false, true, "Menaje y Reformas", "MENAJE Y REFORMAS", new Guid("00000000-0000-0000-0002-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0003-000000000001"), "#F59E0B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 1, 1, "fuel", false, true, "Combustible", "COMBUSTIBLE", new Guid("00000000-0000-0000-0003-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0003-000000000002"), "#F59E0B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 2, 1, "bus", false, true, "Transporte Público", "TRANSPORTE PUBLICO", new Guid("00000000-0000-0000-0003-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0003-000000000003"), "#F59E0B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 1, "navigation", false, true, "Taxi y VTC", "TAXI Y VTC", new Guid("00000000-0000-0000-0003-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0003-000000000004"), "#F59E0B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 4, 1, "circle-parking", false, true, "Parking y Peajes", "PARKING Y PEAJES", new Guid("00000000-0000-0000-0003-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0003-000000000005"), "#F59E0B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 5, 1, "wrench", false, true, "Mantenimiento Vehículo", "MANTENIMIENTO VEHICULO", new Guid("00000000-0000-0000-0003-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0004-000000000001"), "#8B5CF6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 1, 1, "play", false, true, "Streaming Multimedia", "STREAMING MULTIMEDIA", new Guid("00000000-0000-0000-0004-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0004-000000000002"), "#8B5CF6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 2, 1, "cloud", false, true, "Software y Nube", "SOFTWARE Y NUBE", new Guid("00000000-0000-0000-0004-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0004-000000000003"), "#8B5CF6", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 1, "dumbbell", false, true, "Gimnasio y Membresías", "GIMNASIO Y MEMBRESIAS", new Guid("00000000-0000-0000-0004-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0005-000000000001"), "#EC4899", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 1, 1, "plane", false, true, "Viajes y Vacaciones", "VIAJES Y VACACIONES", new Guid("00000000-0000-0000-0005-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0005-000000000002"), "#EC4899", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 2, 1, "film", false, true, "Cine y Teatro", "CINE Y TEATRO", new Guid("00000000-0000-0000-0005-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0005-000000000003"), "#EC4899", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 1, "shirt", false, true, "Ropa y Calzado", "ROPA Y CALZADO", new Guid("00000000-0000-0000-0005-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0005-000000000004"), "#EC4899", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 4, 1, "trophy", false, true, "Hobbies y Deporte", "HOBBIES Y DEPORTE", new Guid("00000000-0000-0000-0005-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0006-000000000001"), "#06B6D4", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 1, 1, "pill", false, true, "Farmacia", "FARMACIA", new Guid("00000000-0000-0000-0006-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0006-000000000002"), "#06B6D4", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 2, 1, "stethoscope", false, true, "Médicos y Dentista", "MEDICOS Y DENTISTA", new Guid("00000000-0000-0000-0006-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0006-000000000003"), "#06B6D4", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 1, "glasses", false, true, "Óptica y Cuidado Personal", "OPTICA Y CUIDADO PERSONAL", new Guid("00000000-0000-0000-0006-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0007-000000000001"), "#22C55E", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 1, 2, "banknote", false, true, "Nómina y Salario", "NOMINA Y SALARIO", new Guid("00000000-0000-0000-0007-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0007-000000000002"), "#22C55E", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 2, 2, "trending-up", false, true, "Dividendos e Intereses", "DIVIDENDOS E INTERESES", new Guid("00000000-0000-0000-0007-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0007-000000000003"), "#22C55E", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 2, "arrow-down-left", false, true, "Transferencias Recibidas", "TRANSFERENCIAS RECIBIDAS", new Guid("00000000-0000-0000-0007-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0007-000000000004"), "#22C55E", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 4, 2, "undo", false, true, "Devoluciones", "DEVOLUCIONES", new Guid("00000000-0000-0000-0007-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0008-000000000001"), "#64748B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 1, 1, "file-text", false, true, "IRPF", "IRPF", new Guid("00000000-0000-0000-0008-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0008-000000000002"), "#64748B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 2, 1, "landmark", false, true, "IBI y Tasas Municipales", "IBI Y TASAS MUNICIPALES", new Guid("00000000-0000-0000-0008-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0008-000000000003"), "#64748B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 3, 1, "shield", false, true, "Seguros Generales", "SEGUROS GENERALES", new Guid("00000000-0000-0000-0008-000000000000"), null, null },
                    { new Guid("00000000-0000-0000-0008-000000000004"), "#64748B", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 1, null, 4, 1, "percent", false, true, "Comisiones Bancarias", "COMISIONES BANCARIAS", new Guid("00000000-0000-0000-0008-000000000000"), null, null }
                });

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_account_clean_merchant",
                table: "bank_transactions",
                columns: new[] { "bank_account_id", "clean_merchant_name" });

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_category_id",
                table: "bank_transactions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_bank_transactions_subcategory_id",
                table: "bank_transactions",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_normalized_name",
                table: "categories",
                column: "normalized_name");

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_category_id",
                table: "categories",
                column: "parent_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_categories_user_parent_normalized",
                table: "categories",
                columns: new[] { "user_id", "parent_category_id", "normalized_name" });

            migrationBuilder.CreateIndex(
                name: "ix_categorization_rules_target_category_id",
                table: "categorization_rules",
                column: "target_category_id");

            migrationBuilder.CreateIndex(
                name: "ix_categorization_rules_target_subcategory_id",
                table: "categorization_rules",
                column: "target_subcategory_id");

            migrationBuilder.CreateIndex(
                name: "ix_categorization_rules_user_active_priority",
                table: "categorization_rules",
                columns: new[] { "user_id", "is_active", "priority" });

            migrationBuilder.CreateIndex(
                name: "ix_transaction_split_items_category_id",
                table: "transaction_split_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_split_items_subcategory_id",
                table: "transaction_split_items",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "ix_transaction_split_items_transaction_id",
                table: "transaction_split_items",
                column: "bank_transaction_id");

            migrationBuilder.AddForeignKey(
                name: "fk_bank_transactions_categories_category_id",
                table: "bank_transactions",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_bank_transactions_categories_subcategory_id",
                table: "bank_transactions",
                column: "subcategory_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bank_transactions_categories_category_id",
                table: "bank_transactions");

            migrationBuilder.DropForeignKey(
                name: "fk_bank_transactions_categories_subcategory_id",
                table: "bank_transactions");

            migrationBuilder.DropTable(
                name: "categorization_rules");

            migrationBuilder.DropTable(
                name: "transaction_split_items");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropIndex(
                name: "ix_bank_transactions_account_clean_merchant",
                table: "bank_transactions");

            migrationBuilder.DropIndex(
                name: "ix_bank_transactions_category_id",
                table: "bank_transactions");

            migrationBuilder.DropIndex(
                name: "ix_bank_transactions_subcategory_id",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "categorization_source",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "clean_merchant_name",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "expense_flexibility",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "expense_nature",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "external_platform",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "has_splits",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "is_manual",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "is_recurring",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "pending_platform_enrichment",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "recurrence_period",
                table: "bank_transactions");

            migrationBuilder.DropColumn(
                name: "subcategory_id",
                table: "bank_transactions");
        }
    }
}
