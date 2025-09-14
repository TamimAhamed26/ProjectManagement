using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedByIdToAssignedTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedById",
                table: "AssignedTasks",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignedTasks_AssignedById",
                table: "AssignedTasks",
                column: "AssignedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignedTasks_AspNetUsers_AssignedById",
                table: "AssignedTasks",
                column: "AssignedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignedTasks_AspNetUsers_AssignedById",
                table: "AssignedTasks");

            migrationBuilder.DropIndex(
                name: "IX_AssignedTasks_AssignedById",
                table: "AssignedTasks");

            migrationBuilder.DropColumn(
                name: "AssignedById",
                table: "AssignedTasks");
        }
    }
}
