using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetworkMapper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScanResultsRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScansResults_Scans_ScanId",
                table: "ScansResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScansResults",
                table: "ScansResults");

            migrationBuilder.RenameTable(
                name: "ScansResults",
                newName: "ScanResults");

            migrationBuilder.RenameIndex(
                name: "IX_ScansResults_ScanId",
                table: "ScanResults",
                newName: "IX_ScanResults_ScanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScanResults",
                table: "ScanResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScanResults_Scans_ScanId",
                table: "ScanResults",
                column: "ScanId",
                principalTable: "Scans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ScanResults_Scans_ScanId",
                table: "ScanResults");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ScanResults",
                table: "ScanResults");

            migrationBuilder.RenameTable(
                name: "ScanResults",
                newName: "ScansResults");

            migrationBuilder.RenameIndex(
                name: "IX_ScanResults_ScanId",
                table: "ScansResults",
                newName: "IX_ScansResults_ScanId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ScansResults",
                table: "ScansResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ScansResults_Scans_ScanId",
                table: "ScansResults",
                column: "ScanId",
                principalTable: "Scans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
