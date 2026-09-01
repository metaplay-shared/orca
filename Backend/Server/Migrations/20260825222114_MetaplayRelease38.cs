using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class MetaplayRelease38 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IsSqlite())
            {
                migrationBuilder.RenameIndex(
                    name: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchasePlatformUserId_ProductId",
                    table: "ServerDrivenInAppPurchases",
                    newName: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchaseP~5FF29ED");
            }
            else
            {
                migrationBuilder.RenameIndex(
                    name: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchasePlatformU",
                    table: "ServerDrivenInAppPurchases",
                    newName: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchaseP~5FF29ED");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.IsSqlite())
            {
                migrationBuilder.RenameIndex(
                    name: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchaseP~5FF29ED",
                    table: "ServerDrivenInAppPurchases",
                    newName: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchasePlatformUserId_ProductId");
            }
            else
            {
                migrationBuilder.RenameIndex(
                    name: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchaseP~5FF29ED",
                    table: "ServerDrivenInAppPurchases",
                    newName: "IX_ServerDrivenInAppPurchases_PurchasePlatform_PurchasePlatformU");
            }
        }
    }
}
