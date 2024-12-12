using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Hero",
                columns: table => new
                {
                    HeroDbId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountDbId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hero", x => x.HeroDbId);
                });

            migrationBuilder.CreateTable(
                name: "Item",
                columns: table => new
                {
                    ItemDbId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    EquipSlot = table.Column<int>(type: "int", nullable: false),
                    AccountDbId = table.Column<long>(type: "bigint", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    OwnerDbId = table.Column<int>(type: "int", nullable: false),
                    EnchantCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Item", x => x.ItemDbId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Hero_AccountDbId",
                table: "Hero",
                column: "AccountDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_AccountDbId",
                table: "Item",
                column: "AccountDbId");

            migrationBuilder.CreateIndex(
                name: "IX_Item_OwnerDbId",
                table: "Item",
                column: "OwnerDbId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Hero");

            migrationBuilder.DropTable(
                name: "Item");
        }
    }
}
