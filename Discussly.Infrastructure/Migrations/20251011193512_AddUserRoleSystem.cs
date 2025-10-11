using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discussly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Role",
                table: "Users",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
