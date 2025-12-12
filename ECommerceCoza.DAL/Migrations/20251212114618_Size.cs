using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceCoza.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Size : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "ProductVariants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "ProductVariants");
        }
    }
}
