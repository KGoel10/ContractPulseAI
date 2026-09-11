using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractPulseAI.API.Migrations
{
    /// <inheritdoc />
    public partial class Initialmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClientRFPs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Client_Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    RFP_Prompt = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    RFP_Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SOW_Link = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    RFP_Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Created_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRFPs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ResourcePricings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Position = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Allocation_Hours = table.Column<int>(type: "int", nullable: false),
                    Pricing_Per_Hour = table.Column<int>(type: "int", nullable: false),
                    Created_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcePricings", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClientRFPs");

            migrationBuilder.DropTable(
                name: "ResourcePricings");
        }
    }
}
