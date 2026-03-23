using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FoodSafetyTracker.MVC.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Premises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Town = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RiskRating = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Premises", x => x.Id));

            migrationBuilder.CreateTable(
                name: "Inspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    PremisesId = table.Column<int>(type: "INTEGER", nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false),
                    Outcome = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inspections", x => x.Id);
                    table.ForeignKey("FK_Inspections_Premises_PremisesId", x => x.PremisesId, "Premises", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FollowUps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true),
                    InspectionId = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUps", x => x.Id);
                    table.ForeignKey("FK_FollowUps_Inspections_InspectionId", x => x.InspectionId, "Inspections", "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_Inspections_PremisesId", "Inspections", "PremisesId");
            migrationBuilder.CreateIndex("IX_FollowUps_InspectionId", "FollowUps", "InspectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("FollowUps");
            migrationBuilder.DropTable("Inspections");
            migrationBuilder.DropTable("Premises");
        }
    }
}
