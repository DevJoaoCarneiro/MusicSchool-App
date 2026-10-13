using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInstruments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Instruments",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { new Guid("a1f5d9c2-3e4b-4b7a-8f1e-1a2b3c4d5e01"), "Violão popular e clássico", "Violão" },
                    { new Guid("b2f6d9c3-3e4b-4b7a-8f1e-1a2b3c4d5e02"), "Guitarra elétrica e acústica", "Guitarra" },
                    { new Guid("c3f7d9c4-3e4b-4b7a-8f1e-1a2b3c4d5e03"), "Bateria acústica", "Bateria" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Instruments",
                keyColumn: "Id",
                keyValue: new Guid("a1f5d9c2-3e4b-4b7a-8f1e-1a2b3c4d5e01"));

            migrationBuilder.DeleteData(
                table: "Instruments",
                keyColumn: "Id",
                keyValue: new Guid("b2f6d9c3-3e4b-4b7a-8f1e-1a2b3c4d5e02"));

            migrationBuilder.DeleteData(
                table: "Instruments",
                keyColumn: "Id",
                keyValue: new Guid("c3f7d9c4-3e4b-4b7a-8f1e-1a2b3c4d5e03"));
        }
    }
}
