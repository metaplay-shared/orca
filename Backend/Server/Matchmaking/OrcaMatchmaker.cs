using Game.Logic;
using Metaplay.Cloud.Entity;
using Metaplay.Cloud.RuntimeOptions;
using Metaplay.Core;
using Metaplay.Core.Model;
using Metaplay.Server.Matchmaking;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.FormattableString;

namespace Game.Server.Matchmaking
{
    [RuntimeOptions("Matchmaker", isStatic: true, "Options for asynchronous matchmaker.")]
    public class MatchmakerOptions : AsyncMatchmakerOptionsBase { }

    [EntityConfig]
    public class AsyncMatchmakerConfig : AsyncMatchmakerConfigBase
    {
        /// <inheritdoc />
        public override EntityKind          EntityKind              => EntityKindGame.AsyncMatchmaker;
        /// <inheritdoc />
        public override Type                EntityActorType         => typeof(OrcaAsyncMatchmakerActor);
    }

    [MetaSerializable]
    public struct OrcaMatchmakerPlayerModel : IAsyncMatchmakerPlayerModel
    {
        [MetaMember(1)] public EntityId         PlayerId                { get; set; }
        [MetaMember(2)] public int              DefenseMmr              { get; set; }

        public OrcaMatchmakerPlayerModel(EntityId playerId, int defenseMmr)
        {
            PlayerId             = playerId;
            DefenseMmr           = defenseMmr;
        }

        /// <inheritdoc />
        public string GetDashboardSummary()
        {
            return Invariant($"MMR: {DefenseMmr}");
        }

        public static OrcaMatchmakerPlayerModel? TryCreateModel(PlayerModel player) {
            return new OrcaMatchmakerPlayerModel(
                player.PlayerId,
                player.MatchMakingScore
            );
        }
    }

    [MetaSerializableDerived(1)]
    public class OrcaMatchmakerQuery : AsyncMatchmakerQueryBase
    {
        public OrcaMatchmakerQuery() : base() { }

        public OrcaMatchmakerQuery(EntityId attackerId, int attackMmr) : base(attackerId, attackMmr)
        {
        }
    }

    public class OrcaAsyncMatchmakerActor : AsyncMatchmakerActorBase<
        PlayerModel,
        OrcaMatchmakerPlayerModel,
        OrcaMatchmakerQuery,
        OrcaAsyncMatchmakerActor.OrcaAsyncMatchmakerActorState,
        MatchmakerOptions
    >
    {
        [MetaSerializableDerived(1)]
        [SupportedSchemaVersions(2, 2)]
        public class OrcaAsyncMatchmakerActorState : MatchmakerStateBase { }

        /// <inheritdoc />
        public OrcaAsyncMatchmakerActor(EntityId entityId) : base(entityId) { }

        /// <inheritdoc />
        protected override string MatchmakerName => "Competitive Matchmaker";
        /// <inheritdoc />
        protected override string MatchmakerDescription => "Matches players based on their skill.";

        /// <inheritdoc />
        protected override bool EnableDatabaseScan => true;

        /// <inheritdoc />
        protected override BucketFillLevelThreshold PlayerIgnoreUpdateInsertThreshold => BucketFillLevelThreshold.HighPopulation;

        /// <inheritdoc />
        protected override OrcaMatchmakerPlayerModel? TryCreateModel(PlayerModel model)
            => OrcaMatchmakerPlayerModel.TryCreateModel(model);

        /// <inheritdoc />
        protected override bool CheckPossibleMatchForQuery(OrcaMatchmakerQuery query, in OrcaMatchmakerPlayerModel player, int numRetries, out float quality)
        {
            quality = 1000 - Math.Abs(player.DefenseMmr - query.AttackMmr);
            return true;
        }
    }
}

