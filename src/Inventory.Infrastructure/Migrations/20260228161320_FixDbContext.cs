using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Inventory.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Party",
                columns: table => new
                {
                    PartyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PatryName = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Party", x => x.PartyId);
                });

            migrationBuilder.CreateTable(
                name: "Designs",
                columns: table => new
                {
                    DesignId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Quality = table.Column<int>(type: "integer", nullable: true),
                    DesignNo = table.Column<string>(type: "text", nullable: true),
                    PartyId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designs", x => x.DesignId);
                    table.ForeignKey(
                        name: "FK_Designs_Party_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Party",
                        principalColumn: "PartyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignPlates",
                columns: table => new
                {
                    DesignPlateId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DesignId = table.Column<int>(type: "integer", nullable: false),
                    PlateName = table.Column<string>(type: "text", nullable: true),
                    PlateNo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignPlates", x => x.DesignPlateId);
                    table.ForeignKey(
                        name: "FK_DesignPlates_Designs_DesignId",
                        column: x => x.DesignId,
                        principalTable: "Designs",
                        principalColumn: "DesignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DesignMatchings",
                columns: table => new
                {
                    DesignMatchingId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlateId = table.Column<int>(type: "integer", nullable: false),
                    MatchingNo = table.Column<int>(type: "integer", nullable: false),
                    Colour = table.Column<string>(type: "text", nullable: true),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: true),
                    DesignPlateId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DesignMatchings", x => x.DesignMatchingId);
                    table.ForeignKey(
                        name: "FK_DesignMatchings_DesignPlates_DesignPlateId",
                        column: x => x.DesignPlateId,
                        principalTable: "DesignPlates",
                        principalColumn: "DesignPlateId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DesignMatchings_DesignPlateId",
                table: "DesignMatchings",
                column: "DesignPlateId");

            migrationBuilder.CreateIndex(
                name: "IX_DesignPlates_DesignId",
                table: "DesignPlates",
                column: "DesignId");

            migrationBuilder.CreateIndex(
                name: "IX_Designs_PartyId",
                table: "Designs",
                column: "PartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DesignMatchings");

            migrationBuilder.DropTable(
                name: "DesignPlates");

            migrationBuilder.DropTable(
                name: "Designs");

            migrationBuilder.DropTable(
                name: "Party");
        }
    }
}
