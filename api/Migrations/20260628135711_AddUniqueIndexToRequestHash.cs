using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToRequestHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RequestHash",
                table: "Jobs",
                type: "char(64)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_RequestHash_Unique",
                table: "Jobs",
                column: "RequestHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Jobs_RequestHash_Unique",
                table: "Jobs");

            migrationBuilder.AlterColumn<string>(
                name: "RequestHash",
                table: "Jobs",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "char(64)");
        }
    }
}
