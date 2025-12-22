using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCoza.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPhoneNumberNormalizedIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumberNormalized",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers",
                column: "PhoneNumberNormalized",
                filter: "[PhoneNumberNormalized] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumberNormalized",
                table: "AspNetUsers");
        }
    }
}
