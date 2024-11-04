using Microsoft.EntityFrameworkCore.Migrations;

namespace FitSharp.Migrations
{
    public partial class AddRestrictDeleteBehavior : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerMemberships_Customers_CustomerId",
                table: "CustomerMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerMemberships_Memberships_MembershipId",
                table: "CustomerMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Gyms_GymId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Gyms_Cities_CityId",
                table: "Gyms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Gyms_GymId",
                table: "Rooms");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerMemberships_Customers_CustomerId",
                table: "CustomerMemberships",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerMemberships_Memberships_MembershipId",
                table: "CustomerMemberships",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Gyms_GymId",
                table: "Equipments",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Gyms_Cities_CityId",
                table: "Gyms",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Gyms_GymId",
                table: "Rooms",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_ClassTypes_ClassTypeId",
                table: "Sessions",
                column: "ClassTypeId",
                principalTable: "ClassTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Customers_CustomerId",
                table: "Sessions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Employees_InstructorId",
                table: "Sessions",
                column: "InstructorId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Rooms_RoomId",
                table: "Sessions",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerMemberships_Customers_CustomerId",
                table: "CustomerMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerMemberships_Memberships_MembershipId",
                table: "CustomerMemberships");

            migrationBuilder.DropForeignKey(
                name: "FK_Equipments_Gyms_GymId",
                table: "Equipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Gyms_Cities_CityId",
                table: "Gyms");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Gyms_GymId",
                table: "Rooms");

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

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Cities_CityId",
                table: "AspNetUsers",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerMemberships_Customers_CustomerId",
                table: "CustomerMemberships",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerMemberships_Memberships_MembershipId",
                table: "CustomerMemberships",
                column: "MembershipId",
                principalTable: "Memberships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Equipments_Gyms_GymId",
                table: "Equipments",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Gyms_Cities_CityId",
                table: "Gyms",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Gyms_GymId",
                table: "Rooms",
                column: "GymId",
                principalTable: "Gyms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

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
    }
}