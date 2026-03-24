using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDesignIdFromProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Program_Designs_DesignId",
                table: "Program");

            migrationBuilder.DropIndex(
                name: "IX_Program_DesignId",
                table: "Program");

            migrationBuilder.DropColumn(
                name: "DesignId",
                table: "Program");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DesignId",
                table: "Program",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Program_DesignId",
                table: "Program",
                column: "DesignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Program_Designs_DesignId",
                table: "Program",
                column: "DesignId",
                principalTable: "Designs",
                principalColumn: "DesignId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
