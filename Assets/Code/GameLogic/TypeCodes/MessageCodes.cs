// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

namespace Game.Logic
{
    /// <summary>
    /// Message code registry for game-specific messages.
    /// </summary>
    public static class MessageCodes
    {
        // Matchmaking (client->server)
        public const int OrcaMatchingRequest = 18100;

        // Matchmaking (server->client)
        public const int OrcaMatchingResponse = 18101;

        // Matchmaking (server internal)
        public const int InternalPlayerGetBattleAttackParamsRequest = 18102;
        public const int InternalPlayerGetBattleAttackParamsResponse = 18103;

        // Player Leagues (client->server)
        public const int PlayerJoinOrcaLeagueRequest = 18200;

        // Player Leagues (server->client)
        public const int PlayerJoinOrcaLeagueResponse = 18201;

        // Add your message codes here
        // Example:
        //  public const int InternalMessageFoo = 15400;
    }
}
