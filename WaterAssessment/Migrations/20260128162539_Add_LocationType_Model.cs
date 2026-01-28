using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WaterAssessment.Migrations
{
    /// <inheritdoc />
    public partial class Add_LocationType_Model : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCanal",
                table: "Locations");

            migrationBuilder.AddColumn<int>(
                name: "LocationTypeID",
                table: "Locations",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "LocationTypes",
                columns: table => new
                {
                    LocationTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTypes", x => x.LocationTypeID);
                });

            migrationBuilder.InsertData(
                table: "LocationTypes",
                columns: new[] { "LocationTypeID", "Title" },
                values: new object[,]
                {
                    { 1, "کانال" },
                    { 2, "زهکش" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_LocationTypeID",
                table: "Locations",
                column: "LocationTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_LocationTypes_LocationTypeID",
                table: "Locations",
                column: "LocationTypeID",
                principalTable: "LocationTypes",
                principalColumn: "LocationTypeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_LocationTypes_LocationTypeID",
                table: "Locations");

            migrationBuilder.DropTable(
                name: "LocationTypes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_LocationTypeID",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "LocationTypeID",
                table: "Locations");

            migrationBuilder.AddColumn<bool>(
                name: "IsCanal",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
