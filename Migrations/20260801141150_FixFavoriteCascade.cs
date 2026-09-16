using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifiedFreelanceArtisansDirectory.Migrations
{
    /// <inheritdoc />
    public partial class FixFavoriteCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_ServiceProviderProfiles_ServiceProviderProfileId",
                table: "Favorites");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_ServiceProviderProfiles_ServiceProviderProfileId",
                table: "Favorites",
                column: "ServiceProviderProfileId",
                principalTable: "ServiceProviderProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_ServiceProviderProfiles_ServiceProviderProfileId",
                table: "Favorites");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_ServiceProviderProfiles_ServiceProviderProfileId",
                table: "Favorites",
                column: "ServiceProviderProfileId",
                principalTable: "ServiceProviderProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
