// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Cloud.Entity;
using Metaplay.Cloud.Sharding;
using Metaplay.Core;
using Metaplay.Core.League;
using Metaplay.Core.League.Player;
using Metaplay.Core.Model;
using Metaplay.Server.League;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Logic;
using Game.Logic.TypeCodes;
using Metaplay.Cloud.RuntimeOptions;
using static System.FormattableString;

namespace Game.Server.League
{
    [MetaSerializableDerived(1)]
    [SupportedSchemaVersions(1, 1)]
    public class OrcaLeagueManagerState : LeagueManagerActorStateBase
    {
    }
    public class IdlerLeagueManagerRegistry : LeagueManagerRegistry
    {
        public override IReadOnlyList<LeagueInfo> LeagueInfos { get; } = new LeagueInfo[]
        {
            new LeagueInfo(
                leagueManagerId: EntityId.Create(EntityKindCloudCore.LeagueManager,
                    0), // Value here should be the same as leagueId in the PlayerActor league integration. The ids must start from 0 and increase by 1 for each league.
                clientSlot: ClientSlotGame.OrcaLeague,
                participantKind: EntityKindCore.Player,
                managerActorType: typeof(OrcaLeagueManagerActor),
                optionsType: typeof(LeagueManagerOptions),
                divisionActorType: typeof(OrcaPlayerDivisionActor))
        };
    }
    
    [RuntimeOptions("Leagues", true, "Options for the leagues manager service.")]
    public class LeagueManagerOptions : LeagueManagerOptionsBase {
        public int DivisionDesiredParticipantCount { get; set; }
        public int DivisionMaxParticipantCount { get; set; }
    }
    
    [PlayerLeaguesEnabledCondition]
    [EntityConfig]
    internal sealed class OrcaLeagueManagerConfig : PersistedEntityConfig
    {
        public override EntityKind        EntityKind           => EntityKindCloudCore.LeagueManager;
        public override Type              EntityActorType      => typeof(OrcaLeagueManagerActor);
        public override NodeSetPlacement  NodeSetPlacement     => NodeSetPlacement.Service;
        public override IShardingStrategy ShardingStrategy     => ShardingStrategies.CreateSingletonService();
        public override TimeSpan          ShardShutdownTimeout => TimeSpan.FromSeconds(10);
    }
    
    public class OrcaLeagueManagerActor : LeagueManagerActorBase<OrcaLeagueManagerState, PersistedDivision, LeagueManagerOptions>
    {
        protected override EntityKind ParticipantEntityKind => EntityKindCore.Player;

        readonly int numRanks = 5;

        public OrcaLeagueManagerActor(EntityId entityId) : base(entityId) { }

        protected override Task<OrcaLeagueManagerState> InitializeNew()
        {
            OrcaLeagueManagerState state = new OrcaLeagueManagerState();
            return Task.FromResult(state);
        }

        protected override Task<ParticipantJoinRequestResult> SolveParticipantInitialPlacement(int currentSeason, EntityId participant, LeagueJoinRequestPayloadBase payload)
        {
            // Always start from rank 0.
            return Task.FromResult(new ParticipantJoinRequestResult(true, 0));
        }

        /// <inheritdoc />
        protected override ParticipantSeasonPlacementResult SolveLocalSeasonPlacement(int lastSeason, int nextSeason, int currentRank, IDivisionParticipantConclusionResult conclusionResult)
        {
            OrcaPlayerDivisionConclusionResult playerResult = conclusionResult as OrcaPlayerDivisionConclusionResult;

            if (playerResult == null)
                throw new ArgumentNullException(nameof(conclusionResult), $"Conclusion result was null or not castable to a {nameof(OrcaPlayerDivisionConclusionResult)}");

            // Demote / remove inactive players
            //if (currentRank == 0 && playerResult.PlayerScore.NumProducerUpgrades == 0)
            //    return ParticipantSeasonPlacementResult.ForRemoval();


            // Promote the top 5 players.
            if (currentRank < numRanks - 1 && playerResult.LeaderboardPlacementIndex <= 5)
                return ParticipantSeasonPlacementResult.ForRank(currentRank + 1);

            int leaderBoardPlacementFromBottom = playerResult.LeaderboardNumPlayers - playerResult.LeaderboardPlacementIndex - 1;

            // Demote the bottom 8 players if division was at least half full.
            if (currentRank > 0 && leaderBoardPlacementFromBottom < 8 && playerResult.LeaderboardNumPlayers > (Options.DivisionDesiredParticipantCount / 2))
                return ParticipantSeasonPlacementResult.ForRank(currentRank - 1);

            // Otherwise stay in the same rank.
            return ParticipantSeasonPlacementResult.ForRank(currentRank);
        }

        /// <inheritdoc />
        protected override Dictionary<EntityId, ParticipantSeasonPlacementResult> SolveGlobalSeasonPlacement(int lastSeason, int nextSeason, Dictionary<EntityId, GlobalRankUpParticipantData> allParticipants)
        {
            Dictionary<EntityId, ParticipantSeasonPlacementResult> results = new Dictionary<EntityId, ParticipantSeasonPlacementResult>();

            // Legend rank size is 50% of diamond players or max 100, whichever is smaller.
            int legendRankSize = Math.Min(Options.DivisionDesiredParticipantCount, (int)(allParticipants.Count * 0.5f));

            IEnumerable<(EntityId Id, GlobalRankUpParticipantData Data)> topParticipants = allParticipants.OrderByDescending(
                (x) => ((OrcaPlayerDivisionConclusionResult)x.Value.ConclusionResult).PlayerScore.MergeScore).Select((x) => (x.Key, x.Value)).Take(legendRankSize);

            foreach ((EntityId Id, GlobalRankUpParticipantData Data) topParticipant in topParticipants)
            {
                // Top players go to legend
                results.Add(topParticipant.Id, ParticipantSeasonPlacementResult.ForRank(numRanks - 1));
            }

            foreach (KeyValuePair<EntityId, GlobalRankUpParticipantData> participant in allParticipants)
            {
                // Skip top participants
                if(results.ContainsKey(participant.Key))
                    continue;

                OrcaPlayerDivisionConclusionResult playerResult = participant.Value.ConclusionResult as OrcaPlayerDivisionConclusionResult;

                if (playerResult == null)
                    throw new NullReferenceException($"Conclusion result was null or not castable to a {nameof(OrcaPlayerDivisionConclusionResult)}");

                int leaderBoardPlacementFromBottom = playerResult.LeaderboardNumPlayers - playerResult.LeaderboardPlacementIndex - 1;

                // Demote the bottom 8 players of diamond if division was at least half full.
                if (participant.Value.CurrentRank == numRanks - 2 && leaderBoardPlacementFromBottom < 8 && playerResult.LeaderboardNumPlayers > (Options.DivisionDesiredParticipantCount / 2))
                    results.Add(participant.Key, ParticipantSeasonPlacementResult.ForRank(participant.Value.CurrentRank - 1));
                else
                    results.Add(participant.Key, ParticipantSeasonPlacementResult.ForRank(numRanks - 2));
            }

            return results;
        }

        /// <inheritdoc />
        protected override LeagueRankDetails GetRankDetails(int rank, int season)
        {
            switch (rank)
            {
                case 0:
                    return new LeagueRankDetails("Bronze", "The lowest tier.", Options.DivisionDesiredParticipantCount);
                case 1:
                    return new LeagueRankDetails("Silver", "For the best of the worst.", Options.DivisionDesiredParticipantCount);
                case 2:
                    return new LeagueRankDetails("Gold", "Good mergers live here.", Options.DivisionDesiredParticipantCount);
                case 3:
                    return new LeagueRankDetails("Diamond", "The best mergers.", Options.DivisionDesiredParticipantCount);
                case 4:
                    return new LeagueRankDetails("Legend", "Only the best of the best.", Options.DivisionDesiredParticipantCount);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), "Given rank is out of range of known ranks");
            }
        }

        /// <inheritdoc />
        protected override LeagueRankUpStrategy GetRankUpStrategy(int rank, int season)
        {
            switch (rank)
            {
                case 0:
                    return new LeagueRankUpStrategy();
                case 1:
                    return new LeagueRankUpStrategy(preferNonEmptyDivisions: true);
                case 2:
                    return new LeagueRankUpStrategy(preferNonEmptyDivisions: true);
                case 3:
                    return new LeagueRankUpStrategy(preferNonEmptyDivisions: true, rankUpMethod: LeagueRankUpMethod.Global);
                case 4:
                    return new LeagueRankUpStrategy(isSingleDivision: true, rankUpMethod: LeagueRankUpMethod.Global);
                default:
                    throw new ArgumentOutOfRangeException(nameof(rank), "Given rank is out of range of known ranks");
            }
        }

        /// <inheritdoc />
        protected override LeagueSeasonDetails GetSeasonDetails(int season)
        {
            return new LeagueSeasonDetails(Invariant($"Season {season}"), "A normal orca season.", numRanks);
        }

        /// <inheritdoc />
        protected override LeagueDetails GetLeagueDetails()
        {
            return new LeagueDetails("Daily Merge Competition", "Players get points based on how many merged they do per season.");
        }
    }
}

