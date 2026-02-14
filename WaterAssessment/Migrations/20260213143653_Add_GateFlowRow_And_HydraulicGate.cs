using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterAssessment.Migrations
{
    /// <inheritdoc />
    public partial class Add_GateFlowRow_And_HydraulicGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_CurrentMeters_CurrentMeterID",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Propellers_PropellerID",
                table: "Assessments");

            migrationBuilder.AddColumn<int>(
                name: "MeasurementFormType",
                table: "Locations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SectionCount",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectionNumber",
                table: "FormValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "PropellerID",
                table: "Assessments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CurrentMeterID",
                table: "Assessments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "ManualTotalFlow",
                table: "Assessments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeasurementFormType",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "HydraulicGates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LocationID = table.Column<int>(type: "int", nullable: false),
                    GateNumber = table.Column<int>(type: "int", nullable: false),
                    DischargeCoefficient = table.Column<double>(type: "float", nullable: false),
                    Width = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HydraulicGates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HydraulicGates_Locations_LocationID",
                        column: x => x.LocationID,
                        principalTable: "Locations",
                        principalColumn: "LocationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GateFlowRows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssessmentID = table.Column<int>(type: "int", nullable: false),
                    HydraulicGateID = table.Column<int>(type: "int", nullable: false),
                    OpeningHeight = table.Column<double>(type: "float", nullable: false),
                    UpstreamHead = table.Column<double>(type: "float", nullable: false),
                    CalculatedFlow = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GateFlowRows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GateFlowRows_Assessments_AssessmentID",
                        column: x => x.AssessmentID,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GateFlowRows_HydraulicGates_HydraulicGateID",
                        column: x => x.HydraulicGateID,
                        principalTable: "HydraulicGates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GateFlowRows_AssessmentID_HydraulicGateID",
                table: "GateFlowRows",
                columns: new[] { "AssessmentID", "HydraulicGateID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GateFlowRows_HydraulicGateID",
                table: "GateFlowRows",
                column: "HydraulicGateID");

            migrationBuilder.CreateIndex(
                name: "IX_HydraulicGates_LocationID",
                table: "HydraulicGates",
                column: "LocationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_CurrentMeters_CurrentMeterID",
                table: "Assessments",
                column: "CurrentMeterID",
                principalTable: "CurrentMeters",
                principalColumn: "CurrentMeterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Propellers_PropellerID",
                table: "Assessments",
                column: "PropellerID",
                principalTable: "Propellers",
                principalColumn: "PropellerID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_CurrentMeters_CurrentMeterID",
                table: "Assessments");

            migrationBuilder.DropForeignKey(
                name: "FK_Assessments_Propellers_PropellerID",
                table: "Assessments");

            migrationBuilder.DropTable(
                name: "GateFlowRows");

            migrationBuilder.DropTable(
                name: "HydraulicGates");

            migrationBuilder.DropColumn(
                name: "MeasurementFormType",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SectionCount",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "SectionNumber",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "ManualTotalFlow",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "MeasurementFormType",
                table: "Assessments");

            migrationBuilder.AlterColumn<int>(
                name: "PropellerID",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CurrentMeterID",
                table: "Assessments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_CurrentMeters_CurrentMeterID",
                table: "Assessments",
                column: "CurrentMeterID",
                principalTable: "CurrentMeters",
                principalColumn: "CurrentMeterID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Propellers_PropellerID",
                table: "Assessments",
                column: "PropellerID",
                principalTable: "Propellers",
                principalColumn: "PropellerID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
