using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class MetaplayRelease31 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LiveOpsTimelineManagers",
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
                    table.PrimaryKey("PK_LiveOpsTimelineManagers", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "StatisticsEvents",
                columns: table => new
                {
                    UniqueKey = table.Column<string>(type: "varchar(128)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "DateTime(3)", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticsEvents", x => x.UniqueKey);
                });

            migrationBuilder.CreateTable(
                name: "StatisticsPages",
                columns: table => new
                {
                    UniqueKey = table.Column<string>(type: "varchar(128)", nullable: false),
                    ResolutionName = table.Column<string>(type: "varchar(128)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "DateTime", nullable: false),
                    EndTime = table.Column<DateTime>(type: "DateTime", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Payload = table.Column<byte[]>(type: "longblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticsPages", x => x.UniqueKey);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsEvents_Timestamp",
                table: "StatisticsEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsPages_CreatedAt",
                table: "StatisticsPages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsPages_EndTime",
                table: "StatisticsPages",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_StatisticsPages_StartTime",
                table: "StatisticsPages",
                column: "StartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LiveOpsTimelineManagers");

            migrationBuilder.DropTable(
                name: "StatisticsEvents");

            migrationBuilder.DropTable(
                name: "StatisticsPages");
        }
    }
}
