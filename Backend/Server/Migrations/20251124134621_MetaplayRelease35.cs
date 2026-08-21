using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class MetaplayRelease35 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<sbyte>(
                name: "DeletionSource",
                table: "Players",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<sbyte>(
                name: "LifecycleStatus",
                table: "Players",
                type: "tinyint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledForDeletionAt",
                table: "Players",
                type: "DateTime",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionSource",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "ScheduledForDeletionAt",
                table: "Players");
        }
    }
}
