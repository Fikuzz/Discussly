using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Discussly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeMediaCommentStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaAttachment_Comments_CommentId",
                table: "MediaAttachment");

            migrationBuilder.DropIndex(
                name: "IX_MediaAttachment_CommentId",
                table: "MediaAttachment");

            migrationBuilder.DropColumn(
                name: "CommentId",
                table: "MediaAttachment");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "MediaAttachment",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(34)",
                oldMaxLength: 34);

            migrationBuilder.AddColumn<string>(
                name: "MediaFileName",
                table: "Comments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaFileName",
                table: "Comments");

            migrationBuilder.AlterColumn<string>(
                name: "Discriminator",
                table: "MediaAttachment",
                type: "character varying(34)",
                maxLength: 34,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(21)",
                oldMaxLength: 21);

            migrationBuilder.AddColumn<Guid>(
                name: "CommentId",
                table: "MediaAttachment",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaAttachment_CommentId",
                table: "MediaAttachment",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAttachment_Comments_CommentId",
                table: "MediaAttachment",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
