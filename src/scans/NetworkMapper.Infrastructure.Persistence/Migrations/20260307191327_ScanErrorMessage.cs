using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NetworkMapper.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ScanErrorMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "Scans",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "Scans");
        }
    }
}
