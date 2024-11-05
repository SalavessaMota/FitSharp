using Microsoft.EntityFrameworkCore.Migrations;

namespace FitSharp.Migrations
{
    public partial class FixMembershipEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Memberships");
        }
    }
}