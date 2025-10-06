using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class RemoveGuilds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuildDiscoveryPoolPages",
                columns: table => new
                {
                    PageId = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    FirstEntryTimestamp = table.Column<DateTime>(type: "DateTime", nullable: false),
                    LastEntryTimestamp = table.Column<DateTime>(type: "DateTime", nullable: false),
                    OwnerId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false),
                    SegmentSequentialId = table.Column<int>(type: "INTEGER", nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    GuildId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    InviteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildInviteCodes", x => x.InviteCode);
                });

            migrationBuilder.CreateTable(
                name: "GuildNameSearches",
                columns: table => new
                {
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    NamePart = table.Column<string>(type: "varchar(32)", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    CachedDisplayName = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: true),
                    PersistedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    SchemaVersion = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.EntityId);
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
        }
    }
}
