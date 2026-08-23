using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vantage.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFindingsAndCveCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CveCacheEntries",
                columns: table => new
                {
                    CpeUri = table.Column<string>(type: "text", nullable: false),
                    CveId = table.Column<string>(type: "text", nullable: false),
                    CvssScore = table.Column<double>(type: "double precision", nullable: false),
                    CachedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CveCacheEntries", x => new { x.CpeUri, x.CveId });
                });

            migrationBuilder.CreateTable(
                name: "ScanRiskAssessmentFindings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScanRiskAssessmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Port = table.Column<int>(type: "integer", nullable: false),
                    Service = table.Column<string>(type: "text", nullable: false),
                    Product = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<string>(type: "text", nullable: true),
                    Cpe = table.Column<string>(type: "text", nullable: true),
                    MatchedCves = table.Column<string>(type: "text", nullable: false),
                    CvssScore = table.Column<double>(type: "double precision", nullable: false),
                    KevFlag = table.Column<bool>(type: "boolean", nullable: false),
                    MatchConfidence = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanRiskAssessmentFindings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScanRiskAssessmentFindings_ScanRiskAssessments_ScanRiskAsse~",
                        column: x => x.ScanRiskAssessmentId,
                        principalTable: "ScanRiskAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScanRiskAssessmentFindings_ScanRiskAssessmentId",
                table: "ScanRiskAssessmentFindings",
                column: "ScanRiskAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CveCacheEntries");

            migrationBuilder.DropTable(
                name: "ScanRiskAssessmentFindings");
        }
    }
}
