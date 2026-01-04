using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterAssessment.Migrations
{
    /// <inheritdoc />
    public partial class Modified_PropellerNumber_To_PropellerName_And_Added_New_Property : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeviceNumber",
                table: "Propellers",
                newName: "PropellerName");

            migrationBuilder.RenameColumn(
                name: "BValue",
                table: "Propellers",
                newName: "B1");

            migrationBuilder.RenameColumn(
                name: "AValue",
                table: "Propellers",
                newName: "A1");

            migrationBuilder.AddColumn<double>(
                name: "A2",
                table: "Propellers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "A3",
                table: "Propellers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "B2",
                table: "Propellers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "B3",
                table: "Propellers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mode",
                table: "Propellers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "TransitionPoint1",
                table: "Propellers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TransitionPoint2",
                table: "Propellers",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "A2",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "A3",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "B2",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "B3",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "Mode",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "TransitionPoint1",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "TransitionPoint2",
                table: "Propellers");

            migrationBuilder.RenameColumn(
                name: "PropellerName",
                table: "Propellers",
                newName: "DeviceNumber");

            migrationBuilder.RenameColumn(
                name: "B1",
                table: "Propellers",
                newName: "BValue");

            migrationBuilder.RenameColumn(
                name: "A1",
                table: "Propellers",
                newName: "AValue");
        }
    }
}
