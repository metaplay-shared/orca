using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class MetaplayRelease29 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_LeagueParticipantDivisionAssociations",
                table: "LeagueParticipantDivisionAssociations");

            migrationBuilder.DropIndex(
                name: "IX_LeagueParticipantDivisionAssociations_LeagueId",
                table: "LeagueParticipantDivisionAssociations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeagueParticipantDivisionAssociations",
                table: "LeagueParticipantDivisionAssociations",
                columns: new[] { "LeagueId", "ParticipantId" });

            migrationBuilder.CreateTable(
                name: "KeyManagers",
                columns: table => new
                {
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    PersistedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyManagers", x => x.EntityId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeyManagers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeagueParticipantDivisionAssociations",
                table: "LeagueParticipantDivisionAssociations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeagueParticipantDivisionAssociations",
                table: "LeagueParticipantDivisionAssociations",
                column: "ParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipantDivisionAssociations_LeagueId",
                table: "LeagueParticipantDivisionAssociations",
                column: "LeagueId");
        }
    }
}
