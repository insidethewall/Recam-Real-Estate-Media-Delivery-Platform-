using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recam_Real_Estate_Media_Delivery_Platform_.Migrations
{
    /// <inheritdoc />
    public partial class InitialTables2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotographerId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AgentId",
                table: "AspNetUsers",
                column: "AgentId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhotographerId",
                table: "AspNetUsers",
                column: "PhotographerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Agents_AgentId",
                table: "AspNetUsers",
                column: "AgentId",
                principalTable: "Agents",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_PhotographyCompanies_PhotographerId",
                table: "AspNetUsers",
                column: "PhotographerId",
                principalTable: "PhotographyCompanies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Agents_AgentId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_PhotographyCompanies_PhotographerId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AgentId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhotographerId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AgentId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhotographerId",
                table: "AspNetUsers");
        }
    }
}
