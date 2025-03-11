using System;
using System.Numerics;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PharmaceuticalManagerBot.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "medbot");

            migrationBuilder.CreateTable(
                name: "ActivePharmIngedients",
                schema: "medbot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    api = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivePharmIngedients", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "MedTypes",
                schema: "medbot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedTypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "medbot",
                columns: table => new
                {
                    uid = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    telegram_id = table.Column<BigInteger>(type: "numeric", nullable: false),
                    telegram_chat_id = table.Column<BigInteger>(type: "numeric", nullable: false),
                    check_req = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "Medicines",
                schema: "medbot",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    api_id = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<int>(type: "integer", nullable: false),
                    expiry_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicines", x => x.id);
                    table.ForeignKey(
                        name: "FK_Medicines_ActivePharmIngedients_api_id",
                        column: x => x.api_id,
                        principalSchema: "medbot",
                        principalTable: "ActivePharmIngedients",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medicines_MedTypes_type_id",
                        column: x => x.type_id,
                        principalSchema: "medbot",
                        principalTable: "MedTypes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Medicines_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "medbot",
                        principalTable: "Users",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_api_id",
                schema: "medbot",
                table: "Medicines",
                column: "api_id");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_type_id",
                schema: "medbot",
                table: "Medicines",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Medicines_user_id",
                schema: "medbot",
                table: "Medicines",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_telegram_id",
                schema: "medbot",
                table: "Users",
                column: "telegram_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_uid",
                schema: "medbot",
                table: "Users",
                column: "uid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Medicines",
                schema: "medbot");

            migrationBuilder.DropTable(
                name: "ActivePharmIngedients",
                schema: "medbot");

            migrationBuilder.DropTable(
                name: "MedTypes",
                schema: "medbot");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "medbot");
        }
    }
}
