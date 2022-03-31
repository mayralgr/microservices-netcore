using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mango.Services.OrderAPI.Migrations
{
    public partial class FixOrderDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentrStatus",
                table: "OrderHeaders",
                newName: "PaymentStatus");

            migrationBuilder.AddColumn<int>(
                name: "CartTotalItems",
                table: "OrderHeaders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartTotalItems",
                table: "OrderHeaders");

            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "OrderHeaders",
                newName: "PaymentrStatus");
        }
    }
}
