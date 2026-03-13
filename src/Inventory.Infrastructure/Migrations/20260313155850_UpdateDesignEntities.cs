using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDesignEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Designs_Party_PartyId",
                table: "Designs");

            migrationBuilder.AlterColumn<int>(
                name: "PartyId",
                table: "Designs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Designs_Party_PartyId",
                table: "Designs",
                column: "PartyId",
                principalTable: "Party",
                principalColumn: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Designs_Party_PartyId",
                table: "Designs");

            migrationBuilder.AlterColumn<int>(
                name: "PartyId",
                table: "Designs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Designs_Party_PartyId",
                table: "Designs",
                column: "PartyId",
                principalTable: "Party",
                principalColumn: "PartyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
