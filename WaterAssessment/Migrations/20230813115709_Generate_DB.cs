using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterAssessment.Migrations
{
    /// <inheritdoc />
    public partial class Generate_DB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    AreaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.AreaID);
                });

            migrationBuilder.CreateTable(
                name: "CurrentMeters",
                columns: table => new
                {
                    CurrentMeterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentMeterName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentMeters", x => x.CurrentMeterID);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "Propellers",
                columns: table => new
                {
                    PropellerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AValue = table.Column<double>(type: "float", nullable: false),
                    BValue = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propellers", x => x.PropellerID);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    LocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Place = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AreaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationID);
                    table.ForeignKey(
                        name: "FK_Locations_Areas_AreaID",
                        column: x => x.AreaID,
                        principalTable: "Areas",
                        principalColumn: "AreaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    AssessmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timer = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Inserted = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getdate()"),
                    Echelon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Openness = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalFlow = table.Column<double>(type: "float", nullable: false),
                    IsCanal = table.Column<bool>(type: "bit", nullable: false),
                    LocationID = table.Column<int>(type: "int", nullable: false),
                    CurrentMeterID = table.Column<int>(type: "int", nullable: false),
                    PropellerID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.AssessmentID);
                    table.ForeignKey(
                        name: "FK_Assessments_CurrentMeters_CurrentMeterID",
                        column: x => x.CurrentMeterID,
                        principalTable: "CurrentMeters",
                        principalColumn: "CurrentMeterID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assessments_Propellers_PropellerID",
                        column: x => x.PropellerID,
                        principalTable: "Propellers",
                        principalColumn: "PropellerID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentEmployees",
                columns: table => new
                {
                    AssessmentID = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentEmployees", x => new { x.AssessmentID, x.EmployeeID });
                    table.ForeignKey(
                        name: "FK_AssessmentEmployees_Assessments_AssessmentID",
                        column: x => x.AssessmentID,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentEmployees_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormValues",
                columns: table => new
                {
                    FormValueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Distance = table.Column<double>(type: "float", nullable: false),
                    Depth = table.Column<double>(type: "float", nullable: false),
                    RadianPerTime_1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadianPerTime_2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadianPerTime_3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssessmentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormValues", x => x.FormValueID);
                    table.ForeignKey(
                        name: "FK_FormValues_Assessments_AssessmentID",
                        column: x => x.AssessmentID,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEmployees_EmployeeID",
                table: "AssessmentEmployees",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_CurrentMeterID",
                table: "Assessments",
                column: "CurrentMeterID");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_LocationID",
                table: "Assessments",
                column: "LocationID");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_PropellerID",
                table: "Assessments",
                column: "PropellerID");

            migrationBuilder.CreateIndex(
                name: "IX_FormValues_AssessmentID",
                table: "FormValues",
                column: "AssessmentID");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_AreaID",
                table: "Locations",
                column: "AreaID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentEmployees");

            migrationBuilder.DropTable(
                name: "FormValues");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "CurrentMeters");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "Propellers");

            migrationBuilder.DropTable(
                name: "Areas");
        }
    }
}
