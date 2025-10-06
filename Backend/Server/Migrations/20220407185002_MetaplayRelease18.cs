using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    public partial class MetaplayRelease18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InAppPurchaseSubscriptions",
                columns: table => new
                {
                    PlayerAndOriginalTransactionId = table.Column<string>(type: "varchar(530)", maxLength: 530, nullable: false),
                    PlayerId = table.Column<string>(type: "varchar(64)", nullable: false),
                    OriginalTransactionId = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    SubscriptionInfo = table.Column<byte[]>(type: "longblob", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InAppPurchaseSubscriptions", x => x.PlayerAndOriginalTransactionId);
                });

            migrationBuilder.CreateTable(
                name: "StatsCollectors",
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
                    table.PrimaryKey("PK_StatsCollectors", x => x.EntityId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InAppPurchaseSubscriptions_OriginalTransactionId",
                table: "InAppPurchaseSubscriptions",
                column: "OriginalTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_InAppPurchaseSubscriptions_PlayerId",
                table: "InAppPurchaseSubscriptions",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InAppPurchaseSubscriptions");

            migrationBuilder.DropTable(
                name: "StatsCollectors");
        }
    }
}
