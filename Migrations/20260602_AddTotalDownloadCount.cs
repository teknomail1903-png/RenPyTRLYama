using Microsoft.EntityFrameworkCore.Migrations;

namespace RenPyTRLauncher.Migrations
{
    public partial class AddTotalDownloadCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TotalDownloadCount",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalDownloadCount",
                table: "Users");
        }
    }
}
