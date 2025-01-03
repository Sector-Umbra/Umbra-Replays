using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SUReplays.Migrations
{
    /// <inheritdoc />
    public partial class InitalMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Replays",
                columns: table => new
                {
                    FileLocation = table.Column<string>(type: "TEXT", nullable: false),
                    Participants = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replays", x => x.FileLocation);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Replays");
        }
    }
}
