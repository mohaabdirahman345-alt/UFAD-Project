using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifiedFreelanceArtisansDirectory.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceProviderNotificationTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NotificationsViewedOn",
                table: "ServiceProviderProfiles",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationsViewedOn",
                table: "ServiceProviderProfiles");
        }
    }
}
