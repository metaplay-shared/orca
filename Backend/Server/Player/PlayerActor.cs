// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Game.Logic;
using Metaplay.Cloud.Entity;
using Metaplay.Core;
using Metaplay.Server;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Logic.LiveOpsEvents;
using Game.Server.GuildDiscovery;
using Metaplay.Cloud.Persistence;
using Metaplay.Core.EventLog;
using Metaplay.Core.Guild;
using Metaplay.Core.League;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Player;
using Metaplay.Server.Database;
using Metaplay.Server.GuildDiscovery;
using Metaplay.Server.League;
using static System.FormattableString;
using ClientSlotGame = Game.Logic.TypeCodes.ClientSlotGame;

namespace Game.Server.Player
{
    /// <summary>
    /// Persisted representation of a player. Used when storing in a database.
    /// </summary>
    public class PersistedPlayer : PersistedPlayerBase
    {
    }

    [EntityConfig]
    public class PlayerConfig : PlayerConfigBase
    {
        public override Type EntityActorType => typeof(PlayerActor);
    }

    /// <summary>
    /// Entity actor class representing a player.
    /// </summary>
    public sealed class PlayerActor : PlayerActorBase<PlayerModel, PersistedPlayer>, IPlayerModelServerListener
    {
        DefaultPlayerLeagueIntegrationHandler<OrcaDivisionClientState> _orcaLeagueIntegration;
        public PlayerActor(EntityId playerId) : base(playerId)
        {
            _orcaLeagueIntegration = Leagues.CreateLeagueIntegrationHandler<DefaultPlayerLeagueIntegrationHandler<OrcaDivisionClientState>>(
                ClientSlotGame.OrcaLeague, 0,
                DefaultPlayerLeagueIntegrationHandler<OrcaDivisionClientState>.Create);
        }

        public sealed class LeagueComponent : LeagueComponentBase
        {
            public LeagueComponent(PlayerActorBase<PlayerModel, PersistedPlayer> playerActor) : base(playerActor) { }
        }

        protected override LeagueComponentBase CreateLeagueComponent()
        {
            return new LeagueComponent(this);
        }

        protected override PersistedPlayer CreatePersisted(EntityId entityId, DateTime persistedAt, byte[] payload, int schemaVersion, bool isFinal)
        {
            return new PersistedPlayer()
            {
                EntityId        = entityId.ToString(),
                PersistedAt     = persistedAt,
                Payload         = payload,
                SchemaVersion   = schemaVersion,
                IsFinal         = isFinal,
            };
        }

        protected override async Task<PlayerModel> InitializeNew() {
            if (_entityId.Value < 5) {
                await EntityAskAsync<SetDeveloperPlayerResponse>(
                    GlobalStateManager.EntityId,
                    new SetDeveloperPlayerRequest(_entityId, true)
                );
            }

            return await base.InitializeNew();
        }

        protected override void OnSwitchedToModel(PlayerModel model)
        {
            model.ServerListener = this;
        }

        protected override string RandomNewPlayerName()
        {
            return Invariant($"Guest {new Random().Next(100_000)}");
        }

        protected override async Task OnSessionStartAsync(PlayerSessionParams start, bool isFirstLogin) {
            if (Model.Level.Level >= 12 || Model.Stats.TotalLogins > 300) {
                SoftReset(Model);

                await MetaDatabase.Get().PurgePlayerIncidentsAsync(MetaTime.Epoch, int.MaxValue);
                await MetaDatabase.Get()
                    .RemoveAllEventLogSegmentsOfEntityAsync<PersistedPlayerEventLogSegment>(_entityId);
                Model.EventLog.RunningEntryId = 1;
                Model.EventLog.OldestAvailableSegmentId = 1;
                Model.EventLog.RunningSegmentId = 1;
                Model.EventLog.PendingSegments = new List<MetaEventLog<PlayerEventLogEntry>.PendingSegment>();
                Model.EventLog.LatestSegmentEntries = new List<PlayerEventLogEntry>();

            } 

            await base.OnSessionStartAsync(start, isFirstLogin);
        }

        public sealed class GuildComponent : GuildComponentBase<PlayerActor>
        {
            public GuildComponent(PlayerActor player) : base(player) { }

            protected override GuildInviterAvatarBase CreateGuildInviterAvatar()
            {
                return new GuildInviterAvatar()
                {
                    PlayerId = Player._entityId,
                    DisplayName = Player.Model.PlayerName,
                };
            }

            protected override GuildMemberPlayerData CreateGuildMemberPlayerData()
            {
                return new GuildMemberPlayerData(
                    displayName: Player.Model.PlayerName
                );
            }

            protected override GuildDiscoveryPlayerContextBase CreateGuildDiscoveryContext()
            {
                return new GuildDiscoveryPlayerContext()
                {
                };
            }

            protected override GuildCreationParamsBase TryCreateGuildCreationParamsFromRequest(GuildCreationRequestParamsBase paramsBase)
            {
                GuildCreationRequestParams requestParams = (GuildCreationRequestParams)paramsBase;

                // no special validation, or custom data. Just pass data thru. The data will
                // be validated again in GuildRequirementsValidator
                return new GuildCreationParams()
                {
                    DisplayName = requestParams?.DisplayName ??  "Cool Guild #" + Util.ObjectToStringInvariant(new Random().Next(100, 999)),
                    Description = requestParams?.Description ?? "",
                };
            }
        }

        protected override GuildComponentBase CreateGuildComponent()
        {
            return new GuildComponent(this);
        }

        public void OnActivityEventScoreAdded(EventId @event, int level, int delta, ResourceModificationContext context) { }

        void IPlayerModelServerListener.OnPlayerXpAdded(int delta) {
        }

        void IPlayerModelServerListener.OnIslandXpAdded(IslandTypeId island, int delta) { }

        void IPlayerModelServerListener.OnBuildingXpAdded(IslandTypeId island, int delta) { }

        void IPlayerModelServerListener.OnHeroXpAdded(HeroTypeId hero, int delta) { }
        public void ItemMerged(ItemModel newItem, int mergeScore) {
            // Add score event
            Leagues[ClientSlotGame.OrcaLeague].EmitDivisionScoreEvent(new OrcaPlayerDivisionMergeScoreEvent(MetaTime.Now, mergeScore));
        }
        protected override Task Initialize() {
            return base.Initialize();
        }

        public void SoftReset(PlayerModel model) {
            KickPlayerIfConnected(PlayerForceKickOwnerReason.AdminAction);

            model.SoftReset();
        }

        [MessageHandler]
        async Task HandlePlayerJoinIdleLeagueRequest(PlayerJoinOrcaLeagueRequest _)
        {
            (DivisionIndex? divisionId, LeagueJoinRefuseReason? refuseReason) = await  Leagues[ClientSlotGame.OrcaLeague].TryJoinPlayerLeague();

            if (divisionId.HasValue)
                PublishMessage(EntityTopic.Owner, PlayerJoinOrcaLeagueResponse.ForSuccess(divisionId.Value));
            else
                PublishMessage(EntityTopic.Owner, PlayerJoinOrcaLeagueResponse.ForFailure(refuseReason.GetValueOrDefault(LeagueJoinRefuseReason.UnknownReason)));
        }
    }
}
