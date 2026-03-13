using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Program",
                columns: table => new
                {
                    ProgramId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramNo = table.Column<string>(type: "text", nullable: false),
                    PartyId = table.Column<int>(type: "integer", nullable: false),
                    Quality = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    MainCut = table.Column<int>(type: "integer", nullable: true),
                    Fold = table.Column<string>(type: "text", nullable: true),
                    Finishing = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true),
                    Round = table.Column<int>(type: "integer", nullable: true),
                    Rate = table.Column<string>(type: "text", nullable: true),
                    DesignId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Program", x => x.ProgramId);
                    table.ForeignKey(
                        name: "FK_Program_Designs_DesignId",
                        column: x => x.DesignId,
                        principalTable: "Designs",
                        principalColumn: "DesignId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Program_Party_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Party",
                        principalColumn: "PartyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramMatchings",
                columns: table => new
                {
                    ProgramMatchingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProgramId = table.Column<int>(type: "integer", nullable: false),
                    DesignId = table.Column<int>(type: "integer", nullable: false),
                    DesignMatchingId = table.Column<int>(type: "integer", nullable: false),
                    PlateId = table.Column<int>(type: "integer", nullable: false),
                    MatchingNo = table.Column<int>(type: "integer", nullable: false),
                    Colour = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramMatchings", x => x.ProgramMatchingId);
                    table.ForeignKey(
                        name: "FK_ProgramMatchings_Program_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Program",
                        principalColumn: "ProgramId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Program_DesignId",
                table: "Program",
                column: "DesignId");

            migrationBuilder.CreateIndex(
                name: "IX_Program_PartyId",
                table: "Program",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramMatchings_ProgramId",
                table: "ProgramMatchings",
                column: "ProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramMatchings");

            migrationBuilder.DropTable(
                name: "Program");
        }
    }
}
