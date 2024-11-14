using Microsoft.EntityFrameworkCore.Migrations;

namespace FitSharp.Migrations
{
    public partial class AddCustomerGroupAndPersonalClassesRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Sessions_GroupClassId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_GroupClassId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "GroupClassId",
                table: "Customers");

            migrationBuilder.CreateTable(
                name: "CustomerGroupClass",
                columns: table => new
                {
                    CustomersId = table.Column<int>(type: "int", nullable: false),
                    GroupClassesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerGroupClass", x => new { x.CustomersId, x.GroupClassesId });
                    table.ForeignKey(
                        name: "FK_CustomerGroupClass_Customers_CustomersId",
                        column: x => x.CustomersId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerGroupClass_Sessions_GroupClassesId",
                        column: x => x.GroupClassesId,
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerGroupClass_GroupClassesId",
                table: "CustomerGroupClass",
                column: "GroupClassesId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerGroupClass");

            migrationBuilder.AddColumn<int>(
                name: "GroupClassId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_GroupClassId",
                table: "Customers",
                column: "GroupClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Sessions_GroupClassId",
                table: "Customers",
                column: "GroupClassId",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
