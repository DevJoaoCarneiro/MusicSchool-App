using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LessonSeriesId",
                table: "Lessons",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LessonSeries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    Weekday = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Time = table.Column<TimeSpan>(type: "time without time zone", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Room = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonSeries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonSeries_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LessonSeries_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_LessonSeriesId",
                table: "Lessons",
                column: "LessonSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSeries_EnrollmentId",
                table: "LessonSeries",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonSeries_TeacherId",
                table: "LessonSeries",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_LessonSeries_LessonSeriesId",
                table: "Lessons",
                column: "LessonSeriesId",
                principalTable: "LessonSeries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_LessonSeries_LessonSeriesId",
                table: "Lessons");

            migrationBuilder.DropTable(
                name: "LessonSeries");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_LessonSeriesId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LessonSeriesId",
                table: "Lessons");
        }
    }
}
