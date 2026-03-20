using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDesignNoDataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MainCut",
                table: "Program",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // ── DesignNo: string → int (manual conversion) ────────────────────

            migrationBuilder.AddColumn<int>(
                name: "DesignNo_New",
                table: "Designs",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Designs""
                SET ""DesignNo_New"" = CAST(regexp_replace(""DesignNo"", '[^0-9]', '', 'g') AS INTEGER)
                WHERE ""DesignNo"" IS NOT NULL
                  AND regexp_replace(""DesignNo"", '[^0-9]', '', 'g') <> '';
            ");

            migrationBuilder.DropColumn(
                name: "DesignNo",
                table: "Designs");

            migrationBuilder.RenameColumn(
                name: "DesignNo_New",
                table: "Designs",
                newName: "DesignNo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MainCut",
                table: "Program",
                type: "integer",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DesignNo",
                table: "Designs",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
