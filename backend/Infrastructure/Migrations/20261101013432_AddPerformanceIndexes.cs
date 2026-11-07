using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lessons_EnrollmentId",
                table: "Lessons");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Name",
                table: "Students",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_EnrollmentId_StartsAt",
                table: "Lessons",
                columns: new[] { "EnrollmentId", "StartsAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_TeacherId_StartsAt",
                table: "Lessons",
                columns: new[] { "TeacherId", "StartsAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_Name",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_EnrollmentId_StartsAt",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_TeacherId_StartsAt",
                table: "Lessons");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_EnrollmentId",
                table: "Lessons",
                column: "EnrollmentId");
        }
    }
}
