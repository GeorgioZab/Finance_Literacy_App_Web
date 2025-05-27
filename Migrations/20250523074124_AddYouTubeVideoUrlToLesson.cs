using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance_Literacy_App_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddYouTubeVideoUrlToLesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "YouTubeVideoUrl",
                table: "Lessons",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YouTubeVideoUrl",
                table: "Lessons");
        }
    }
}
