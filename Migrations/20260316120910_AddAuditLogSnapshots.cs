using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace proekt.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewSnapshot",
                table: "MedicalAuditLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldSnapshot",
                table: "MedicalAuditLogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewSnapshot",
                table: "MedicalAuditLogs");

            migrationBuilder.DropColumn(
                name: "OldSnapshot",
                table: "MedicalAuditLogs");
        }
    }
}
