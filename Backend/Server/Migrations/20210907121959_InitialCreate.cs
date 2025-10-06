using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogEvents",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    Target = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    SourceIpAddress = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    SourceCountryIsoCode = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true),
                    CompressedPayload = table.Column<byte[]>(type: "longblob", nullable: false),
                    CompressionAlgorithm = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogEvents", x => x.EventId);
                });

            migrationBuilder.CreateTable(
                name: "AuthEntries",
                columns: table => new
                {
                    AuthKey = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    HashedAuthToken = table.Column<string>(type: "varchar(160)", nullable: true),
                    PlayerId = table.Column<string>(type: "varchar(64)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthEntries", x => x.AuthKey);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseScanCoordinators",
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
                    table.PrimaryKey("PK_DatabaseScanCoordinators", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseScanWorkers",
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
                    table.PrimaryKey("PK_DatabaseScanWorkers", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "GlobalStates",
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
                    table.PrimaryKey("PK_GlobalStates", x => x.EntityId);
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
                name: "InAppPurchases",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    Event = table.Column<byte[]>(type: "longblob", nullable: false),
                    IsValidReceipt = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<string>(type: "varchar(64)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InAppPurchases", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "MetaInfo",
                columns: table => new
                {
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Timestamp = table.Column<DateTime>(type: "DateTime", nullable: false),
                    MasterVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    NumShards = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaInfo", x => x.Version);
                });

            migrationBuilder.CreateTable(
                name: "PlayerDeletionRecords",
                columns: table => new
                {
                    PlayerId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    ScheduledDeletionAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    DeletionSource = table.Column<string>(type: "varchar(128)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerDeletionRecords", x => x.PlayerId);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEventLogSegments",
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
                    table.PrimaryKey("PK_PlayerEventLogSegments", x => x.GlobalId);
                });

            migrationBuilder.CreateTable(
                name: "PlayerIncidents",
                columns: table => new
                {
                    IncidentId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    PlayerId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Fingerprint = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    SubType = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Reason = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    PersistedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false),
                    Compression = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerIncidents", x => x.IncidentId);
                });

            migrationBuilder.CreateTable(
                name: "PlayerNameSearches",
                columns: table => new
                {
                    NamePart = table.Column<string>(type: "varchar(32)", nullable: false),
                    EntityId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Players",
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
                    table.PrimaryKey("PK_Players", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "StaticGameConfigs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    VersionHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Source = table.Column<string>(type: "varchar(128)", nullable: false),
                    ArchiveBuiltAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    IsArchived = table.Column<bool>(type: "tinyint", nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    TaskId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    FailureInfo = table.Column<string>(type: "TEXT", nullable: true),
                    MetaDataBytes = table.Column<byte[]>(type: "longblob", nullable: true),
                    ArchiveBytes = table.Column<byte[]>(type: "longblob", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaticGameConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogEvents_Source",
                table: "AuditLogEvents",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogEvents_Target",
                table: "AuditLogEvents",
                column: "Target");

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
                name: "IX_PlayerEventLogSegments_OwnerId",
                table: "PlayerEventLogSegments",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerIncidents_PersistedAt",
                table: "PlayerIncidents",
                column: "PersistedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerIncidents_PlayerId",
                table: "PlayerIncidents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerNameSearches_EntityId",
                table: "PlayerNameSearches",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerNameSearches_NamePart_EntityId",
                table: "PlayerNameSearches",
                columns: new[] { "NamePart", "EntityId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogEvents");

            migrationBuilder.DropTable(
                name: "AuthEntries");

            migrationBuilder.DropTable(
                name: "DatabaseScanCoordinators");

            migrationBuilder.DropTable(
                name: "DatabaseScanWorkers");

            migrationBuilder.DropTable(
                name: "GlobalStates");

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
                name: "InAppPurchases");

            migrationBuilder.DropTable(
                name: "MetaInfo");

            migrationBuilder.DropTable(
                name: "PlayerDeletionRecords");

            migrationBuilder.DropTable(
                name: "PlayerEventLogSegments");

            migrationBuilder.DropTable(
                name: "PlayerIncidents");

            migrationBuilder.DropTable(
                name: "PlayerNameSearches");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "StaticGameConfigs");
        }
    }
}
