using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recam_Real_Estate_Media_Delivery_Platform_.Migrations
{
    /// <inheritdoc />
    public partial class updateUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "MediaAssets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_UserId1",
                table: "MediaAssets",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaAssets_AspNetUsers_UserId1",
                table: "MediaAssets",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaAssets_AspNetUsers_UserId1",
                table: "MediaAssets");

            migrationBuilder.DropIndex(
                name: "IX_MediaAssets_UserId1",
                table: "MediaAssets");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "MediaAssets");
        }
    }
}
