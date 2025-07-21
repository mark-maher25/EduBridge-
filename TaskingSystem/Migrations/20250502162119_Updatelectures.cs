using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskingSystem.Migrations
{
    /// <inheritdoc />
    public partial class Updatelectures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "lectureURL",
                table: "lectures",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "lectureURL",
                table: "lectures");
        }
    }
}
