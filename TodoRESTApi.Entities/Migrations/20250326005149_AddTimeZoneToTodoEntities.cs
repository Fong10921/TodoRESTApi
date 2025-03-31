using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoRESTApi.Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeZoneToTodoEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZone",
                table: "Todo",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZone",
                table: "Todo");
        }
    }
}