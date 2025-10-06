using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class MetaplayRelease15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Fingerprint",
                table: "PlayerIncidents",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerIncidents_Fingerprint_PersistedAt",
                table: "PlayerIncidents",
                columns: new[] { "Fingerprint", "PersistedAt" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerIncidents_Fingerprint_PersistedAt",
                table: "PlayerIncidents");

            migrationBuilder.AlterColumn<string>(
                name: "Fingerprint",
                table: "PlayerIncidents",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);
        }
    }
}
