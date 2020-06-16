using Microsoft.EntityFrameworkCore.Migrations;

namespace ExchangeRatesWebApp.Migrations
{
    public partial class ForumCommentAddOriginalMessageColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalMessage",
                table: "ForumComments",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalMessage",
                table: "ForumComments");
        }
    }
}
