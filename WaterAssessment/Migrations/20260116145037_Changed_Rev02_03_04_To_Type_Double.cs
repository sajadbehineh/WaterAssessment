using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterAssessment.Migrations
{
    /// <inheritdoc />
    public partial class Changed_Rev02_03_04_To_Type_Double : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "Rev08",
                table: "FormValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Rev06",
                table: "FormValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Rev02",
                table: "FormValues",
                type: "float",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Rev08",
                table: "FormValues",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Rev06",
                table: "FormValues",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Rev02",
                table: "FormValues",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);
        }
    }
}
