using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FitSharp.Migrations
{
    public partial class RemoveCustomerMembershipTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerMemberships");

            migrationBuilder.AddColumn<int>(
                name: "ClassesRemaining",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "MembershipBeginDate",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "MembershipEndDate",
                table: "Customers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "MembershipIsActive",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClassesRemaining",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MembershipBeginDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MembershipEndDate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MembershipIsActive",
                table: "Customers");

            migrationBuilder.CreateTable(
                name: "CustomerMemberships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeginDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClassesRemaining = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    MembershipId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerMemberships_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerMemberships_Memberships_MembershipId",
                        column: x => x.MembershipId,
                        principalTable: "Memberships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMemberships_CustomerId",
                table: "CustomerMemberships",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerMemberships_MembershipId",
                table: "CustomerMemberships",
                column: "MembershipId");
        }
    }
}
