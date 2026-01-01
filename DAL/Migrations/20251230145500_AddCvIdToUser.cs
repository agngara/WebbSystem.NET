using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCvIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CVs_UserId",
                table: "CVs");

            migrationBuilder.AddColumn<int>(
                name: "CvId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CVs_UserId",
                table: "CVs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CvId",
                table: "AspNetUsers",
                column: "CvId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_CVs_CvId",
                table: "AspNetUsers",
                column: "CvId",
                principalTable: "CVs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_CVs_CvId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_CVs_UserId",
                table: "CVs");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CvId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CvId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_CVs_UserId",
                table: "CVs",
                column: "UserId",
                unique: true);
        }
    }
}
