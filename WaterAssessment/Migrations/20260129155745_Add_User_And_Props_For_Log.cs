using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WaterAssessment.Migrations
{
    /// <inheritdoc />
    public partial class Add_User_And_Props_For_Log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Propellers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Propellers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Propellers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Propellers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LocationTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "LocationTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LocationTypes",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "LocationTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Locations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Locations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Locations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CurrentMeters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "CurrentMeters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "CurrentMeters",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "CurrentMeters",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Areas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserID",
                table: "Areas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Areas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByUserID",
                table: "Areas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "LocationTypeID",
                keyValue: 1,
                columns: new[] { "CreatedAt", "CreatedByUserID", "UpdatedAt", "UpdatedByUserID" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.UpdateData(
                table: "LocationTypes",
                keyColumn: "LocationTypeID",
                keyValue: 2,
                columns: new[] { "CreatedAt", "CreatedByUserID", "UpdatedAt", "UpdatedByUserID" },
                values: new object[] { new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.CreateIndex(
                name: "IX_Propellers_CreatedByUserID",
                table: "Propellers",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Propellers_UpdatedByUserID",
                table: "Propellers",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LocationTypes_CreatedByUserID",
                table: "LocationTypes",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LocationTypes_UpdatedByUserID",
                table: "LocationTypes",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_CreatedByUserID",
                table: "Locations",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_UpdatedByUserID",
                table: "Locations",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_CreatedByUserID",
                table: "Employees",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UpdatedByUserID",
                table: "Employees",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentMeters_CreatedByUserID",
                table: "CurrentMeters",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrentMeters_UpdatedByUserID",
                table: "CurrentMeters",
                column: "UpdatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_CreatedByUserID",
                table: "Areas",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_UpdatedByUserID",
                table: "Areas",
                column: "UpdatedByUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Users_CreatedByUserID",
                table: "Areas",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Areas_Users_UpdatedByUserID",
                table: "Areas",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentMeters_Users_CreatedByUserID",
                table: "CurrentMeters",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_CurrentMeters_Users_UpdatedByUserID",
                table: "CurrentMeters",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_CreatedByUserID",
                table: "Employees",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Users_UpdatedByUserID",
                table: "Employees",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Users_CreatedByUserID",
                table: "Locations",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Users_UpdatedByUserID",
                table: "Locations",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationTypes_Users_CreatedByUserID",
                table: "LocationTypes",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationTypes_Users_UpdatedByUserID",
                table: "LocationTypes",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Propellers_Users_CreatedByUserID",
                table: "Propellers",
                column: "CreatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Propellers_Users_UpdatedByUserID",
                table: "Propellers",
                column: "UpdatedByUserID",
                principalTable: "Users",
                principalColumn: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Users_CreatedByUserID",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_Areas_Users_UpdatedByUserID",
                table: "Areas");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentMeters_Users_CreatedByUserID",
                table: "CurrentMeters");

            migrationBuilder.DropForeignKey(
                name: "FK_CurrentMeters_Users_UpdatedByUserID",
                table: "CurrentMeters");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_CreatedByUserID",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Users_UpdatedByUserID",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Users_CreatedByUserID",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Users_UpdatedByUserID",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationTypes_Users_CreatedByUserID",
                table: "LocationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationTypes_Users_UpdatedByUserID",
                table: "LocationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Propellers_Users_CreatedByUserID",
                table: "Propellers");

            migrationBuilder.DropForeignKey(
                name: "FK_Propellers_Users_UpdatedByUserID",
                table: "Propellers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Propellers_CreatedByUserID",
                table: "Propellers");

            migrationBuilder.DropIndex(
                name: "IX_Propellers_UpdatedByUserID",
                table: "Propellers");

            migrationBuilder.DropIndex(
                name: "IX_LocationTypes_CreatedByUserID",
                table: "LocationTypes");

            migrationBuilder.DropIndex(
                name: "IX_LocationTypes_UpdatedByUserID",
                table: "LocationTypes");

            migrationBuilder.DropIndex(
                name: "IX_Locations_CreatedByUserID",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Locations_UpdatedByUserID",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Employees_CreatedByUserID",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_UpdatedByUserID",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_CurrentMeters_CreatedByUserID",
                table: "CurrentMeters");

            migrationBuilder.DropIndex(
                name: "IX_CurrentMeters_UpdatedByUserID",
                table: "CurrentMeters");

            migrationBuilder.DropIndex(
                name: "IX_Areas_CreatedByUserID",
                table: "Areas");

            migrationBuilder.DropIndex(
                name: "IX_Areas_UpdatedByUserID",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Propellers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "LocationTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CurrentMeters");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "CurrentMeters");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "CurrentMeters");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "CurrentMeters");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "CreatedByUserID",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Areas");

            migrationBuilder.DropColumn(
                name: "UpdatedByUserID",
                table: "Areas");
        }
    }
}
