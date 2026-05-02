using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HHMCore.Data.Migrations;

/// <inheritdoc />
public partial class AddRoomAndTimeSlotAndUpdateCourseAssignment : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Room",
            table: "CourseAssignments");

        migrationBuilder.DropColumn(
            name: "Schedule",
            table: "CourseAssignments");

        migrationBuilder.AddColumn<Guid>(
            name: "RoomId",
            table: "CourseAssignments",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "TimeSlotId",
            table: "CourseAssignments",
            type: "uniqueidentifier",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.CreateTable(
            name: "Rooms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Building = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                RoomType = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rooms", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "TimeSlots",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Days = table.Column<int>(type: "int", nullable: false),
                StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TimeSlots", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CourseAssignments_RoomId",
            table: "CourseAssignments",
            column: "RoomId");

        migrationBuilder.CreateIndex(
            name: "IX_CourseAssignments_TimeSlotId",
            table: "CourseAssignments",
            column: "TimeSlotId");

        migrationBuilder.AddForeignKey(
            name: "FK_CourseAssignments_Rooms_RoomId",
            table: "CourseAssignments",
            column: "RoomId",
            principalTable: "Rooms",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_CourseAssignments_TimeSlots_TimeSlotId",
            table: "CourseAssignments",
            column: "TimeSlotId",
            principalTable: "TimeSlots",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_CourseAssignments_Rooms_RoomId",
            table: "CourseAssignments");

        migrationBuilder.DropForeignKey(
            name: "FK_CourseAssignments_TimeSlots_TimeSlotId",
            table: "CourseAssignments");

        migrationBuilder.DropTable(
            name: "Rooms");

        migrationBuilder.DropTable(
            name: "TimeSlots");

        migrationBuilder.DropIndex(
            name: "IX_CourseAssignments_RoomId",
            table: "CourseAssignments");

        migrationBuilder.DropIndex(
            name: "IX_CourseAssignments_TimeSlotId",
            table: "CourseAssignments");

        migrationBuilder.DropColumn(
            name: "RoomId",
            table: "CourseAssignments");

        migrationBuilder.DropColumn(
            name: "TimeSlotId",
            table: "CourseAssignments");

        migrationBuilder.AddColumn<string>(
            name: "Room",
            table: "CourseAssignments",
            type: "nvarchar(max)",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Schedule",
            table: "CourseAssignments",
            type: "nvarchar(max)",
            nullable: true);
    }
}
