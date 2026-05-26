using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExamEngine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigSnapshotAnalyticsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                schema: "exam",
                table: "ExamSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuizTitle",
                schema: "exam",
                table: "ExamSessions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "exam",
                table: "ExamSessions");

            migrationBuilder.DropColumn(
                name: "QuizTitle",
                schema: "exam",
                table: "ExamSessions");
        }
    }
}
