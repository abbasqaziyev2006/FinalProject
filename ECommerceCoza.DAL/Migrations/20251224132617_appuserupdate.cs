using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCoza.DAL.Migrations
{
    /// <inheritdoc />
    public partial class appuserupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers",
                column: "PhoneNumberNormalized",
                unique: true,
                filter: "[PhoneNumberNormalized] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumberNormalized",
                table: "AspNetUsers",
                column: "PhoneNumberNormalized",
                filter: "[PhoneNumberNormalized] IS NOT NULL");
        }
    }
}
