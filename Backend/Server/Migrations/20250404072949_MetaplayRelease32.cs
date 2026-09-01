using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class MetaplayRelease32 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectorKeys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", nullable: false),
                    ServerName = table.Column<string>(type: "varchar(64)", nullable: false),
                    KeyBytes = table.Column<byte[]>(type: "longblob", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "DateTime", nullable: false),
                    ValidUntil = table.Column<DateTime>(type: "DateTime", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "DateTime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectorKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerDrivenInAppPurchases",
                columns: table => new
                {
                    TransactionId = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    PlayerId = table.Column<string>(type: "varchar(64)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    PurchasePlatform = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    PurchasePlatformUserId = table.Column<string>(type: "varchar(512)", nullable: false),
                    ProductId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerDrivenInAppPurchases", x => x.TransactionId);
                });

            migrationBuilder.CreateTable(
                name: "SteamworksPollers",
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
                    table.PrimaryKey("PK_SteamworksPollers", x => x.EntityId);
                });

            migrationBuilder.CreateTable(
                name: "WebLoginAuthorizations",
                columns: table => new
                {
                    AuthorizationCode = table.Column<string>(type: "char(32)", nullable: false),
                    ClientId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    RedirectUri = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false),
                    S256CodeChallenge = table.Column<byte[]>(type: "binary(32)", nullable: true),
                    State = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    Scope = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    FlowExpiresAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    CodeExchangeExpiresAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    UserInfo = table.Column<byte[]>(type: "longblob", nullable: true),
                    LoginMethod = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebLoginAuthorizations", x => x.AuthorizationCode);
                });

            migrationBuilder.CreateTable(
                name: "WebLoginClientSessions",
                columns: table => new
                {
                    ClientSessionId = table.Column<string>(type: "char(32)", nullable: false),
                    RefreshTokenNonce = table.Column<string>(type: "char(32)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    ClientId = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Scope = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    LoginMethod = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    UserInfo = table.Column<byte[]>(type: "longblob", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebLoginClientSessions", x => x.ClientSessionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataProtectorKeys_ExpiresAt",
                table: "DataProtectorKeys",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_ServerDrivenInAppPurchases_PlayerId",
                table: "ServerDrivenInAppPurchases",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchasePlatformUserId_ProductId",
                table: "ServerDrivenInAppPurchases",
                columns: new[] { "PurchasePlatform", "PurchasePlatformUserId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_WebLoginAuthorizations_FlowExpiresAt",
                table: "WebLoginAuthorizations",
                column: "FlowExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_WebLoginClientSessions_ExpiresAt",
                table: "WebLoginClientSessions",
                column: "ExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataProtectorKeys");

            migrationBuilder.DropTable(
                name: "ServerDrivenInAppPurchases");

            migrationBuilder.DropTable(
                name: "SteamworksPollers");

            migrationBuilder.DropTable(
                name: "WebLoginAuthorizations");

            migrationBuilder.DropTable(
                name: "WebLoginClientSessions");
        }
    }
}
