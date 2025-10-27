using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discussly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditCommentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "Comments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
