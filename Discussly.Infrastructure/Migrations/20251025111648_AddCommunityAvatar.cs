using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discussly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunityAvatar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarFileName",
                table: "Communities",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarFileName",
                table: "Communities");
        }
    }
}
