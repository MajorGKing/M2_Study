using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameServer.Migrations
{
    /// <inheritdoc />
    public partial class init3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Collection",
                columns: table => new
                {
                    OwnerDbId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    ProgressFlag = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collection", x => new { x.OwnerDbId, x.TemplateId });
                    table.ForeignKey(
                        name: "FK_Collection_Hero_OwnerDbId",
                        column: x => x.OwnerDbId,
                        principalTable: "Hero",
                        principalColumn: "HeroDbId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Collection");
        }
    }
}
