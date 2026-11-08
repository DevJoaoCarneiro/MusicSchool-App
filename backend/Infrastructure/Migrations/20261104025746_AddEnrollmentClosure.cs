using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnrollmentClosure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /// <inheritdoc />
            migrationBuilder.AddColumn<string>(
                name: "CloseReason",
                table: "Enrollments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Enrollments",
                type: "date",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"Enrollments\" SET \"Status\" = 'Closed' WHERE \"Status\" = 'Inactive';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Enrollments\" SET \"Status\" = 'Inactive' WHERE \"Status\" = 'Closed';");

            migrationBuilder.DropColumn(
                name: "CloseReason",
                table: "Enrollments");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Enrollments");
        }
    }
}