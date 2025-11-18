using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discussly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EditMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "MediaAttachment");

            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "MediaAttachment");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "MediaAttachment");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "MediaAttachment");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "MediaAttachment",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "MediaAttachment");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "MediaAttachment",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "MediaAttachment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "MediaAttachment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "MediaAttachment",
                type: "text",
                nullable: true);
        }
    }
}
