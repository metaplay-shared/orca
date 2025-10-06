using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class Guilds_Leagues_Matchmaker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Divisions",
                columns: table => new
                {
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    PersistedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: true),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "GuildDiscoveryPoolPages",
                columns: table => new
                {
                    PageId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildDiscoveryPoolPages", x => x.PageId);
                });

            migrationBuilder.CreateTable(
                name: "GuildEventLogSegments",
                columns: table => new
                {
                    GlobalId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    OwnerId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    SegmentSequentialId = table.Column<int>(type: "INTEGER", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false),
                    FirstEntryTimestamp = table.Column<DateTime>(type: "DateTime", nullable: false),
                    LastEntryTimestamp = table.Column<DateTime>(type: "DateTime", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildEventLogSegments", x => x.GlobalId);
                });

            migrationBuilder.CreateTable(
                name: "GuildInviteCodes",
                columns: table => new
                {
                    InviteCode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    GuildId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    InviteId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInviteCodes", x => x.InviteCode);
                });

            migrationBuilder.CreateTable(
                name: "GuildNameSearches",
                columns: table => new
                {
                    NamePart = table.Column<string>(type: "varchar(32)", nullable: false),
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    PersistedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: true),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false),
                    CachedDisplayName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "LeagueManagers",
                columns: table => new
                {
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    PersistedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: true),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueManagers", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "LeagueParticipantDivisionAssociations",
                columns: table => new
                {
                    ParticipantId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    LeagueId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    DivisionId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    LeagueStateRevision = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueParticipantDivisionAssociations", x => x.ParticipantId);
                });

            migrationBuilder.CreateTable(
                name: "Matchmakers",
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
                    table.PrimaryKey("PK_Matchmakers", x => x.EntityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildEventLogSegments_OwnerId",
                table: "GuildEventLogSegments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildNameSearches_EntityId",
                table: "GuildNameSearches",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildNameSearches_NamePart_EntityId",
                table: "GuildNameSearches",
                columns: new[] { "NamePart", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipantDivisionAssociations_DivisionId",
                table: "LeagueParticipantDivisionAssociations",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipantDivisionAssociations_LeagueId",
                table: "LeagueParticipantDivisionAssociations",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipantDivisionAssociations_LeagueStateRevision",
                table: "LeagueParticipantDivisionAssociations",
                column: "LeagueStateRevision");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Divisions");

            migrationBuilder.DropTable(
                name: "GuildDiscoveryPoolPages");

            migrationBuilder.DropTable(
                name: "GuildEventLogSegments");

            migrationBuilder.DropTable(
                name: "GuildInviteCodes");

            migrationBuilder.DropTable(
                name: "GuildNameSearches");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "LeagueManagers");

            migrationBuilder.DropTable(
                name: "LeagueParticipantDivisionAssociations");

            migrationBuilder.DropTable(
                name: "Matchmakers");
        }
    }
}
