using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniServerProject.Migrations
{
    /// <inheritdoc />
    public partial class RefineServerLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ushort>(
                name: "AfterStamina",
                table: "stage_giveup_logs",
                type: "smallint unsigned",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<ushort>(
                name: "RefundStamina",
                table: "stage_giveup_logs",
                type: "smallint unsigned",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<ushort>(
                name: "AfterStamina",
                table: "stage_enter_logs",
                type: "smallint unsigned",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<ushort>(
                name: "ConsumedStamina",
                table: "stage_enter_logs",
                type: "smallint unsigned",
                nullable: false,
                defaultValue: (ushort)0);

            migrationBuilder.AddColumn<ulong>(
                name: "AfterExp",
                table: "stage_clear_logs",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "AfterGold",
                table: "stage_clear_logs",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GainExp",
                table: "stage_clear_logs",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<ulong>(
                name: "GainGold",
                table: "stage_clear_logs",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "RewardId",
                table: "stage_clear_logs",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfterStamina",
                table: "stage_giveup_logs");

            migrationBuilder.DropColumn(
                name: "RefundStamina",
                table: "stage_giveup_logs");

            migrationBuilder.DropColumn(
                name: "AfterStamina",
                table: "stage_enter_logs");

            migrationBuilder.DropColumn(
                name: "ConsumedStamina",
                table: "stage_enter_logs");

            migrationBuilder.DropColumn(
                name: "AfterExp",
                table: "stage_clear_logs");

            migrationBuilder.DropColumn(
                name: "AfterGold",
                table: "stage_clear_logs");

            migrationBuilder.DropColumn(
                name: "GainExp",
                table: "stage_clear_logs");

            migrationBuilder.DropColumn(
                name: "GainGold",
                table: "stage_clear_logs");

            migrationBuilder.DropColumn(
                name: "RewardId",
                table: "stage_clear_logs");
        }
    }
}
