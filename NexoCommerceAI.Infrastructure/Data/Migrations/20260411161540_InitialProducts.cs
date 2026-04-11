using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexoCommerceAI.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Crear la extensión pg_trgm (esto debe hacerse primero)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;", 
                suppressTransaction: true); // Importante: suppressTransaction true
            
            // 2. Ahora proceder con el resto de la migración
            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAt",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<decimal>(
                name: "CompareAtPrice",
                table: "Products",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
            
            // 3. Crear los índices (ahora la extensión ya está disponible)
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId_IsFeatured_IsActive",
                table: "Products",
                columns: new[] { "CategoryId", "IsFeatured", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_IsFeatured_Price",
                table: "Products",
                columns: new[] { "IsActive", "IsFeatured", "Price" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsActive_Stock",
                table: "Products",
                columns: new[] { "IsActive", "Stock" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_IsFeatured",
                table: "Products",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                table: "Products",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "GIN")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Price",
                table: "Products",
                column: "Price");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Sku",
                table: "Products",
                column: "Sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId_IsFeatured_IsActive",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_IsFeatured_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsActive_Stock",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_IsFeatured",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Price",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Sku",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CompareAtPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                table: "Products",
                column: "CreatedAt");
        }
    }
}
