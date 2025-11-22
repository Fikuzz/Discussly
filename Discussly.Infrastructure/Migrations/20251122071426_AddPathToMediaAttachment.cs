using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discussly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPathToMediaAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "MediaAttachment",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Path",
                table: "MediaAttachment");
        }
    }
}
