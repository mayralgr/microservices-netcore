using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mango.Services.ShoppingCartAPI.Migrations
{
    public partial class correctDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartDetails_CartHeader_CartHeaderId",
                table: "CartDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartHeader",
                table: "CartHeader");

            migrationBuilder.RenameTable(
                name: "CartHeader",
                newName: "CartHeaders");

            migrationBuilder.RenameColumn(
                name: "CartId",
                table: "CartHeaders",
                newName: "CartHeaderId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartHeaders",
                table: "CartHeaders",
                column: "CartHeaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartDetails_CartHeaders_CartHeaderId",
                table: "CartDetails",
                column: "CartHeaderId",
                principalTable: "CartHeaders",
                principalColumn: "CartHeaderId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartDetails_CartHeaders_CartHeaderId",
                table: "CartDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CartHeaders",
                table: "CartHeaders");

            migrationBuilder.RenameTable(
                name: "CartHeaders",
                newName: "CartHeader");

            migrationBuilder.RenameColumn(
                name: "CartHeaderId",
                table: "CartHeader",
                newName: "CartId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CartHeader",
                table: "CartHeader",
                column: "CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartDetails_CartHeader_CartHeaderId",
                table: "CartDetails",
                column: "CartHeaderId",
                principalTable: "CartHeader",
                principalColumn: "CartId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
