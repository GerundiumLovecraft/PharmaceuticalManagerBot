using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PharmaceuticalManagerBot.Migrations
{
    /// <inheritdoc />
    public partial class AddedMedTypeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "medbot",
                table: "MedTypes",
                columns: new[] { "id", "type" },
                values: new object[,]
                {
                    { 16, "Спазмолитик" },
                    { 17, "Другое" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "medbot",
                table: "MedTypes",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                schema: "medbot",
                table: "MedTypes",
                keyColumn: "id",
                keyValue: 17);
        }
    }
}
