using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finance_Literacy_App_Web.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteCodeToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "Groups",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "Groups");
        }
    }
}
