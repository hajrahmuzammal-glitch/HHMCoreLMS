using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HHMCore.Data.Migrations;

/// <inheritdoc />
public partial class AddDepartmentIdToCourseAssignment : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "DepartmentId",
            table: "CourseAssignments",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<int>(
            name: "MaxEnrollment",
            table: "CourseAssignments",
            type: "int",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "Section",
            table: "CourseAssignments",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "IX_CourseAssignments_DepartmentId",
            table: "CourseAssignments",
            column: "DepartmentId");

        migrationBuilder.AddForeignKey(
            name: "FK_CourseAssignments_Departments_DepartmentId",
            table: "CourseAssignments",
            column: "DepartmentId",
            principalTable: "Departments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CourseAssignments_Departments_DepartmentId",
            table: "CourseAssignments");

        migrationBuilder.DropIndex(
            name: "IX_CourseAssignments_DepartmentId",
            table: "CourseAssignments");

        migrationBuilder.DropColumn(
            name: "DepartmentId",
            table: "CourseAssignments");

        migrationBuilder.DropColumn(
            name: "MaxEnrollment",
            table: "CourseAssignments");

        migrationBuilder.DropColumn(
            name: "Section",
            table: "CourseAssignments");
    }
}
