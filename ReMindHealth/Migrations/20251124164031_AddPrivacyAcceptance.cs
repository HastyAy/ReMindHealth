using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReMindHealth.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivacyAcceptance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAcceptedPrivacy",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrivacyAcceptedAt",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAcceptedPrivacy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrivacyAcceptedAt",
                table: "AspNetUsers");
        }
    }
}
