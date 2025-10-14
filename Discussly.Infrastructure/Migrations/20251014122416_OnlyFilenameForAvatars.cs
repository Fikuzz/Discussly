using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discussly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OnlyFilenameForAvatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AvatarFileName",
                table: "Users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarFileName",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
