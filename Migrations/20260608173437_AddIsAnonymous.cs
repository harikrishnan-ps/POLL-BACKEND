using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace poll_api.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAnonymous : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymous",
                table: "Polls",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$yNm7x1DZMHz/MK2G5renXuQ4gDf3rCBnJyIFB0WaAYEmb/DAFWeq2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAnonymous",
                table: "Polls");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$37Z14cwcD8pvCzdURk.eA.A0039KtYBQN3goGzZ.V3r7Vu8efUqBO");
        }
    }
}
