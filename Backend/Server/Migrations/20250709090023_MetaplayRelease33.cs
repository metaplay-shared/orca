using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class MetaplayRelease33 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_LeagueParticipantDivisionAssociations_ParticipantId",
                table: "LeagueParticipantDivisionAssociations",
                column: "ParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LeagueParticipantDivisionAssociations_ParticipantId",
                table: "LeagueParticipantDivisionAssociations");
        }
    }
}
