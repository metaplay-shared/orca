using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class MetaplayRelease25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Localizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true),
                    VersionHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    Source = table.Column<string>(type: "varchar(128)", nullable: false),
                    ArchiveBuiltAt = table.Column<DateTime>(type: "DateTime", nullable: false),
                    IsArchived = table.Column<bool>(type: "tinyint", nullable: false),
                    TaskId = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    FailureInfo = table.Column<string>(type: "TEXT", nullable: true),
                    ArchiveBytes = table.Column<byte[]>(type: "longblob", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localizations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Localizations");
        }
    }
}
