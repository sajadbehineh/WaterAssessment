using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterAssessment.Migrations
{
    /// <inheritdoc />
    public partial class Move_IsCanal_From_Assessment_To_Location_And_Add_AssessmentGate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RadianPerTime_1",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "RadianPerTime_2",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "RadianPerTime_3",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "IsCanal",
                table: "Assessments");

            migrationBuilder.DropColumn(
                name: "Openness",
                table: "Assessments");

            migrationBuilder.RenameColumn(
                name: "Depth",
                table: "FormValues",
                newName: "VerticalMeanVelocity");

            migrationBuilder.AddColumn<int>(
                name: "GateCount",
                table: "Locations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanal",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "MeasureTime",
                table: "FormValues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Rev02",
                table: "FormValues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rev06",
                table: "FormValues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rev08",
                table: "FormValues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RowIndex",
                table: "FormValues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SectionArea",
                table: "FormValues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SectionFlow",
                table: "FormValues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SectionWidth",
                table: "FormValues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalDepth",
                table: "FormValues",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Velocity02",
                table: "FormValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Velocity06",
                table: "FormValues",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Velocity08",
                table: "FormValues",
                type: "float",
                nullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Echelon",
                table: "Assessments",
                type: "float",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "AssessmentGates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GateNumber = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<double>(type: "float", nullable: false),
                    AssessmentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentGates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentGates_Assessments_AssessmentID",
                        column: x => x.AssessmentID,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentGates_AssessmentID",
                table: "AssessmentGates",
                column: "AssessmentID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentGates");

            migrationBuilder.DropColumn(
                name: "GateCount",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "IsCanal",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "MeasureTime",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "Rev02",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "Rev06",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "Rev08",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "RowIndex",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "SectionArea",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "SectionFlow",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "SectionWidth",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "TotalDepth",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "Velocity02",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "Velocity06",
                table: "FormValues");

            migrationBuilder.DropColumn(
                name: "Velocity08",
                table: "FormValues");

            migrationBuilder.RenameColumn(
                name: "VerticalMeanVelocity",
                table: "FormValues",
                newName: "Depth");

            migrationBuilder.AddColumn<string>(
                name: "RadianPerTime_1",
                table: "FormValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RadianPerTime_2",
                table: "FormValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RadianPerTime_3",
                table: "FormValues",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Echelon",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<bool>(
                name: "IsCanal",
                table: "Assessments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Openness",
                table: "Assessments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
