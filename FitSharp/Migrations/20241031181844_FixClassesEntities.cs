using Microsoft.EntityFrameworkCore.Migrations;

namespace FitSharp.Migrations
{
    public partial class FixClassesEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonalClasses_ClassTypes_ClassTypeId",
                table: "PersonalClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonalClasses_Customers_CustomerId",
                table: "PersonalClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonalClasses_Employees_InstructorId",
                table: "PersonalClasses");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonalClasses_Rooms_RoomId",
                table: "PersonalClasses");

            migrationBuilder.DropTable(
                name: "CustomerGroupClass");

            migrationBuilder.DropTable(
                name: "GroupClasses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PersonalClasses",
                table: "PersonalClasses");

            migrationBuilder.RenameTable(
                name: "PersonalClasses",
                newName: "Sessions");

            migrationBuilder.RenameIndex(
                name: "IX_PersonalClasses_RoomId",
                table: "Sessions",
                newName: "IX_Sessions_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonalClasses_InstructorId",
                table: "Sessions",
                newName: "IX_Sessions_InstructorId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonalClasses_CustomerId",
                table: "Sessions",
                newName: "IX_Sessions_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_PersonalClasses_ClassTypeId",
                table: "Sessions",
                newName: "IX_Sessions_ClassTypeId");

            migrationBuilder.AddColumn<int>(
                name: "GroupClassId",
                table: "Customers",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "Sessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "Sessions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions",
                column: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_ClassTypes_ClassTypeId",
                table: "Sessions",
                column: "ClassTypeId",
                principalTable: "ClassTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Customers_CustomerId",
                table: "Sessions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Employees_InstructorId",
                table: "Sessions",
                column: "InstructorId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Rooms_RoomId",
                table: "Sessions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Sessions_GroupClassId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_ClassTypes_ClassTypeId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Customers_CustomerId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Employees_InstructorId",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Rooms_RoomId",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Customers_GroupClassId",
                table: "Customers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sessions",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "GroupClassId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Sessions");

            migrationBuilder.RenameTable(
                name: "Sessions",
                newName: "PersonalClasses");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_RoomId",
                table: "PersonalClasses",
                newName: "IX_PersonalClasses_RoomId");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_InstructorId",
                table: "PersonalClasses",
                newName: "IX_PersonalClasses_InstructorId");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_CustomerId",
                table: "PersonalClasses",
                newName: "IX_PersonalClasses_CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Sessions_ClassTypeId",
                table: "PersonalClasses",
                newName: "IX_PersonalClasses_ClassTypeId");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "PersonalClasses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PersonalClasses",
                table: "PersonalClasses",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "GroupClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassTypeId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupClasses_ClassTypes_ClassTypeId",
                        column: x => x.ClassTypeId,
                        principalTable: "ClassTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupClasses_Employees_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupClasses_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerGroupClass_GroupClasses_GroupClassesId",
                        column: x => x.GroupClassesId,
                        principalTable: "GroupClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerGroupClass_GroupClassesId",
                table: "CustomerGroupClass",
                column: "GroupClassesId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupClasses_ClassTypeId",
                table: "GroupClasses",
                column: "ClassTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupClasses_InstructorId",
                table: "GroupClasses",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupClasses_RoomId",
                table: "GroupClasses",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalClasses_ClassTypes_ClassTypeId",
                table: "PersonalClasses",
                column: "ClassTypeId",
                principalTable: "ClassTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalClasses_Customers_CustomerId",
                table: "PersonalClasses",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalClasses_Employees_InstructorId",
                table: "PersonalClasses",
                column: "InstructorId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonalClasses_Rooms_RoomId",
                table: "PersonalClasses",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
