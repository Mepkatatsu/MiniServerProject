using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniServerProject.Migrations
{
    /// <inheritdoc />
    public partial class RefineUserSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ushort>(
                name: "Stamina",
                table: "users",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(short),
                oldType: "smallint");

            migrationBuilder.AlterColumn<ushort>(
                name: "Level",
                table: "users",
                type: "smallint unsigned",
                nullable: false,
                defaultValue: (ushort)1,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)1);

            migrationBuilder.AddColumn<string>(
                name: "CurrentStageId",
                table: "users",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStageId",
                table: "users");

            migrationBuilder.AlterColumn<short>(
                name: "Stamina",
                table: "users",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<short>(
                name: "Level",
                table: "users",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned",
                oldDefaultValue: (ushort)1);
        }
    }
}
