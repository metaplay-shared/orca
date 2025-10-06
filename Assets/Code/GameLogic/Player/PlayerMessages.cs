// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.League;

namespace Game.Logic
{
    /// <summary>
    /// Client requests to join the idle leagues.
    /// Server responds with <see cref="PlayerJoinOrcaLeagueResponse"/>
    /// </summary>
    [MetaMessage(MessageCodes.PlayerJoinOrcaLeagueRequest, MessageDirection.ClientToServer), MessageRoutingRuleOwnedPlayer]
    public class PlayerJoinOrcaLeagueRequest : MetaMessage
    {
        public static PlayerJoinOrcaLeagueRequest Instance { get; } = new PlayerJoinOrcaLeagueRequest();

        PlayerJoinOrcaLeagueRequest() { }
    }

    /// <summary>
    /// Server's response to <see cref="PlayerJoinOrcaLeagueRequest"/>.
    /// If the join was successful <see cref="JoinedDivision"/> is set.
    /// Otherwise <see cref="FailureReason"/> will be set.
    /// </summary>
    [MetaMessage(MessageCodes.PlayerJoinOrcaLeagueResponse, MessageDirection.ServerToClient)]
    public class PlayerJoinOrcaLeagueResponse : MetaMessage
    {
        public bool                   Success        { get; private set; }
        public LeagueJoinRefuseReason FailureReason  { get; private set; }
        public DivisionIndex          JoinedDivision { get; private set; }

        PlayerJoinOrcaLeagueResponse() { }

        public PlayerJoinOrcaLeagueResponse(bool success, LeagueJoinRefuseReason failureReason, DivisionIndex joinedDivision)
        {
            Success        = success;
            FailureReason  = failureReason;
            JoinedDivision = joinedDivision;
        }

        public static PlayerJoinOrcaLeagueResponse ForSuccess(DivisionIndex joinedDivision)
            => new PlayerJoinOrcaLeagueResponse(true, default, joinedDivision);

        public static PlayerJoinOrcaLeagueResponse ForFailure(LeagueJoinRefuseReason failureReason)
            => new PlayerJoinOrcaLeagueResponse(false, failureReason, default);
    }
}
