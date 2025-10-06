// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Game.Logic;
using Game.Server.GuildDiscovery;
using Metaplay.Cloud.Entity;
using Metaplay.Core;
using Metaplay.Core.Guild;
using Metaplay.Core.GuildDiscovery;
using Metaplay.Core.Model;
using Metaplay.Server.Guild;
using Metaplay.Server.GuildDiscovery;
using System;

namespace Game.Server.Guild
{
    [EntityConfig]
    public class GuildConfig : GuildEntityConfigBase
    {
        public override Type EntityActorType => typeof(GuildActor);
    }

    /// <summary>
    /// Entity actor class representing a guild.
    /// </summary>
    public sealed class GuildActor : GuildActorBase<GuildModel>, IGuildModelServerListener
    {
        protected override sealed TimeSpan TickUpdateInterval => TimeSpan.FromSeconds(10);

        public GuildActor(EntityId guildId) : base(guildId)
        {
        }

        protected override void OnSwitchedToModel(GuildModel model)
        {
            model.ServerListener = this;
        }

        protected override bool ShouldAcceptPlayerJoin(EntityId playerId, GuildMemberPlayerDataBase playerData, bool isInvited)
        {
            // If we are too full, don't allow new players
            if (Model.Members.Count >= Model.MaxNumMembers)
                return false;

            // \todo: check custom level requirements etc.
            return true;
        }

        protected override (GuildDiscoveryInfoBase, GuildDiscoveryServerOnlyInfoBase) CreateGuildDiscoveryInfo()
        {
            return
            (
                new GuildDiscoveryInfo(
                    guildId:                _entityId,
                    displayName:            Model.DisplayName,
                    numMembers:             Model.Members.Count,
                    maxNumMembers:          Model.MaxNumMembers
                    ),
                new GuildDiscoveryServerOnlyInfo(
                    guildCreatedAt:         Model.CreatedAt,
                    memberOnlineLatestAt:   Model.GetMemberOnlineLatestAt(timestampNow: MetaTime.Now)
                    )
            );
        }

        protected override sealed void SetupGuildWithCreationParams(GuildCreationParamsBase baseArgs)
        {
            GuildCreationParams args = (GuildCreationParams)baseArgs;

            base.SetupGuildWithCreationParams(args);
        }

        protected override object GetMemberGdprExportExtraData(EntityId memberPlayerId)
        {
            // we don't have extra data
            return null;
        }
    }
}
