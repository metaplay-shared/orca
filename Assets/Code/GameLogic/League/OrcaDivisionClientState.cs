// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.League;
using Metaplay.Core.League.Player;
using Metaplay.Core.Model;

namespace Game.Logic
{
    /// <summary>
    /// An example conclusion result for divisions.
    /// </summary>
    [MetaSerializableDerived(1)]
    public class OrcaPlayerDivisionConclusionResult : PlayerDivisionParticipantConclusionResultBase<PlayerDivisionAvatarBase.Default>
    {
        [MetaMember(1)] public int                      LeaderboardPlacementIndex { get; private set; }
        [MetaMember(2)] public int                      LeaderboardNumPlayers     { get; private set; }
        [MetaMember(3)] public OrcaPlayerDivisionScore  PlayerScore               { get; private set; }

        public OrcaPlayerDivisionConclusionResult(EntityId participantId, PlayerDivisionAvatarBase.Default avatar, int leaderboardPlacementIndex,
            int leaderboardNumPlayers, OrcaPlayerDivisionScore playerScore) : base(participantId, avatar)
        {
            LeaderboardPlacementIndex = leaderboardPlacementIndex;
            LeaderboardNumPlayers     = leaderboardNumPlayers;
            PlayerScore               = playerScore;
        }

        OrcaPlayerDivisionConclusionResult() : base(default, default) { }
    }

    /// <summary>
    /// An example historical entry of a division for the idler leagues.
    /// </summary>
    [MetaSerializableDerived(1)]
    public class OrcaPlayerDivisionHistoryEntry : PlayerDivisionHistoryEntryBase
    {
        /// <summary>
        /// The player's score in the division.
        /// </summary>
        [MetaMember(1)] public OrcaPlayerDivisionScore PlayerScore { get; private set; }

        /// <summary>
        /// The player's placement in the final division leaderboard. 0 is 1st .
        /// </summary>
        [MetaMember(2)] public int LeaderboardPlacementIndex { get; private set; }

        public OrcaPlayerDivisionHistoryEntry(EntityId divisionId, DivisionIndex divisionIndex, IDivisionRewards rewards, OrcaPlayerDivisionScore playerScore,
            int leaderboardPlacementIndex) : base(divisionId, divisionIndex, rewards)
        {
            PlayerScore               = playerScore;
            LeaderboardPlacementIndex = leaderboardPlacementIndex;
        }

        OrcaPlayerDivisionHistoryEntry() : base(EntityId.None, default, null) { }
    }

    /// <summary>
    /// Example state for the leagues that is stored within the PlayerModel.
    /// This contains the current division and a history.
    /// </summary>
    [MetaSerializableDerived(1)]
    public class OrcaDivisionClientState : DivisionClientStateBase<OrcaPlayerDivisionHistoryEntry> { }
}
