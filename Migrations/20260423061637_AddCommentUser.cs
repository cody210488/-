using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace 打球啊.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CourtComments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CourtComments_UserId",
                table: "CourtComments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourtComments_AspNetUsers_UserId",
                table: "CourtComments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourtComments_AspNetUsers_UserId",
                table: "CourtComments");

            migrationBuilder.DropIndex(
                name: "IX_CourtComments_UserId",
                table: "CourtComments");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CourtComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
