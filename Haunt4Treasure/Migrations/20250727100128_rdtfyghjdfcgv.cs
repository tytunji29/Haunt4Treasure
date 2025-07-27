using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haunt4Treasure.Migrations
{
    /// <inheritdoc />
    public partial class rdtfyghjdfcgv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefCode",
                table: "WalletTransactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefCode",
                table: "WalletTransactions",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
