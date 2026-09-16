using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifiedFreelanceArtisansDirectory.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceProviderCV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceProviderCVs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceProviderProfileId = table.Column<int>(type: "int", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceProviderCVs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceProviderCVs_ServiceProviderProfiles_ServiceProviderProfileId",
                        column: x => x.ServiceProviderProfileId,
                        principalTable: "ServiceProviderProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderCVs_ServiceProviderProfileId",
                table: "ServiceProviderCVs",
                column: "ServiceProviderProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceProviderCVs_UploadedAt",
                table: "ServiceProviderCVs",
                column: "UploadedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceProviderCVs");
        }
    }
}
