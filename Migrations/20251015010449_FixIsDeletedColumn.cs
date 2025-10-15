using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recam_Real_Estate_Media_Delivery_Platform_.Migrations
{
    /// <inheritdoc />
    public partial class FixIsDeletedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "MediaAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "ListcaseStatus",
                table: "ListingCases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ListingCases",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ListingCases",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "MediaAssets");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "MediaAssets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ListcaseStatus",
                table: "ListingCases",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsDeleted",
                table: "ListingCases",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ListingCases",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

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
    }
}
