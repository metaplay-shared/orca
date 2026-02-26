using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Migrations
{
    public partial class SegmentSizeEstimator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SegmentEstimates",
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
                    table.PrimaryKey("PK_SegmentEstimates", x => x.EntityId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SegmentEstimates");
        }
    }
}
