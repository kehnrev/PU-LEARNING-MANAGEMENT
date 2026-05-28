using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduTrackAnalytics.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonMaterialMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Lessons",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Lessons",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "Lessons",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "Lessons");
        }
    }
}
