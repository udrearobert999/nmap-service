using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetworkMapper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClerkIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AuthSubject",
                table: "Teams",
                newName: "ClerkOrgId");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_AuthSubject",
                table: "Teams",
                newName: "IX_Teams_ClerkOrgId");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Teams",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Scans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "ScanRiskAssessments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClerkUserId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TeamMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMemberships_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMemberships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Scans_CreatedByUserId",
                table: "Scans",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanRiskAssessments_CreatedByUserId",
                table: "ScanRiskAssessments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_TeamId",
                table: "TeamMemberships",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMemberships_UserId_TeamId",
                table: "TeamMemberships",
                columns: new[] { "UserId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClerkUserId",
                table: "Users",
                column: "ClerkUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ScanRiskAssessments_Users_CreatedByUserId",
                table: "ScanRiskAssessments",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Scans_Users_CreatedByUserId",
                table: "Scans",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScanRiskAssessments_Users_CreatedByUserId",
                table: "ScanRiskAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Scans_Users_CreatedByUserId",
                table: "Scans");

            migrationBuilder.DropTable(
                name: "TeamMemberships");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Scans_CreatedByUserId",
                table: "Scans");

            migrationBuilder.DropIndex(
                name: "IX_ScanRiskAssessments_CreatedByUserId",
                table: "ScanRiskAssessments");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Scans");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "ScanRiskAssessments");

            migrationBuilder.RenameColumn(
                name: "ClerkOrgId",
                table: "Teams",
                newName: "AuthSubject");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_ClerkOrgId",
                table: "Teams",
                newName: "IX_Teams_AuthSubject");
        }
    }
}
