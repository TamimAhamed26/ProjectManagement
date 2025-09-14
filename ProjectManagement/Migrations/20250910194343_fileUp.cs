using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Migrations
{
    /// <inheritdoc />
    public partial class fileUp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferenceFilePath",
                table: "AssignedTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceLink",
                table: "AssignedTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionFilePath",
                table: "AssignedTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubmissionLink",
                table: "AssignedTasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReferenceFilePath",
                table: "AssignedTasks");

            migrationBuilder.DropColumn(
                name: "ReferenceLink",
                table: "AssignedTasks");

            migrationBuilder.DropColumn(
                name: "SubmissionFilePath",
                table: "AssignedTasks");

            migrationBuilder.DropColumn(
                name: "SubmissionLink",
                table: "AssignedTasks");
        }
    }
}
