using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HHMCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixAllCascadeToRestrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_CourseAssignments_CourseAssignmentId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_Students_StudentId",
                table: "AssignmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Departments_DepartmentId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_FeeRecords_Semesters_SemesterId",
                table: "FeeRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FeeRecords_Students_StudentId",
                table: "FeeRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizResults_Students_StudentId",
                table: "QuizResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_CourseAssignments_CourseAssignmentId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Departments_DepartmentId",
                table: "Teachers");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_CourseAssignments_CourseAssignmentId",
                table: "Assignments",
                column: "CourseAssignmentId",
                principalTable: "CourseAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_Students_StudentId",
                table: "AssignmentSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Departments_DepartmentId",
                table: "Courses",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeeRecords_Semesters_SemesterId",
                table: "FeeRecords",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeeRecords_Students_StudentId",
                table: "FeeRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResults_Students_StudentId",
                table: "QuizResults",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_CourseAssignments_CourseAssignmentId",
                table: "Quizzes",
                column: "CourseAssignmentId",
                principalTable: "CourseAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Departments_DepartmentId",
                table: "Teachers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_CourseAssignments_CourseAssignmentId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentSubmissions_Students_StudentId",
                table: "AssignmentSubmissions");

            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Departments_DepartmentId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_FeeRecords_Semesters_SemesterId",
                table: "FeeRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_FeeRecords_Students_StudentId",
                table: "FeeRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_QuizResults_Students_StudentId",
                table: "QuizResults");

            migrationBuilder.DropForeignKey(
                name: "FK_Quizzes_CourseAssignments_CourseAssignmentId",
                table: "Quizzes");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers");

            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Departments_DepartmentId",
                table: "Teachers");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_CourseAssignments_CourseAssignmentId",
                table: "Assignments",
                column: "CourseAssignmentId",
                principalTable: "CourseAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentSubmissions_Students_StudentId",
                table: "AssignmentSubmissions",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Departments_DepartmentId",
                table: "Courses",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Students_StudentId",
                table: "Enrollments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeeRecords_Semesters_SemesterId",
                table: "FeeRecords",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeeRecords_Students_StudentId",
                table: "FeeRecords",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizResults_Students_StudentId",
                table: "QuizResults",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Quizzes_CourseAssignments_CourseAssignmentId",
                table: "Quizzes",
                column: "CourseAssignmentId",
                principalTable: "CourseAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_UserId",
                table: "Students",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_Departments_DepartmentId",
                table: "Students",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_AspNetUsers_UserId",
                table: "Teachers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Departments_DepartmentId",
                table: "Teachers",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
