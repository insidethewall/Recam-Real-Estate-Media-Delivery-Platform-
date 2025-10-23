using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recam_Real_Estate_Media_Delivery_Platform_.Migrations
{
    /// <inheritdoc />
    public partial class moldalBuiding3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentListingCases_ListingCases_ListingCaseId",
                table: "AgentListingCases");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentListingCases_ListingCases_ListingCaseId",
                table: "AgentListingCases",
                column: "ListingCaseId",
                principalTable: "ListingCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentListingCases_ListingCases_ListingCaseId",
                table: "AgentListingCases");

            migrationBuilder.AddForeignKey(
                name: "FK_AgentListingCases_ListingCases_ListingCaseId",
                table: "AgentListingCases",
                column: "ListingCaseId",
                principalTable: "ListingCases",
                principalColumn: "Id");
        }
    }
}
