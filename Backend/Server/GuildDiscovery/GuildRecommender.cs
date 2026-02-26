// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Game.Logic;
using Metaplay.Cloud.Entity;
using Metaplay.Core;
using Metaplay.Core.GuildDiscovery;
using Metaplay.Core.Model;
using Metaplay.Core.Serialization;
using Metaplay.Server.GuildDiscovery;
using System;
using System.Collections.Generic;

namespace Game.Server.GuildDiscovery
{
    /// <summary>
    /// Helper for the common, shared filtering operations.
    /// </summary>
    public static class CommonDiscoveryPoolContextFilters
    {
        public static bool Test(GuildDiscoveryPlayerContext playerContext, GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo)
        {
            // Add custom filters here.
            return true;
        }
    }

    /// <summary>
    /// Helper for common operations and mapping between generic and game-specific types.
    /// </summary>
    public abstract class GameGuildDiscoveryPool : PersistedGuildDiscoveryPool
    {
        protected GameGuildDiscoveryPool(string poolId) : base(poolId)
        {
        }

        public override sealed bool ContextFilter(GuildDiscoveryPlayerContextBase playerContextBase, IGuildDiscoveryPool.GuildInfo info)
        {
            GuildDiscoveryPlayerContext playerContext = (GuildDiscoveryPlayerContext)playerContextBase;
            GuildDiscoveryInfo discoveryInfo = (GuildDiscoveryInfo)info.PublicDiscoveryInfo;
            GuildDiscoveryServerOnlyInfo discoveryServerOnlyInfo = (GuildDiscoveryServerOnlyInfo)info.ServerOnlyDiscoveryInfo;

            // common first
            if (!CommonDiscoveryPoolContextFilters.Test(playerContext, discoveryInfo, discoveryServerOnlyInfo))
                return false;

            // then pool-specific
            return ContextFilter(playerContext, discoveryInfo, discoveryServerOnlyInfo);
        }

        public override sealed bool Filter(IGuildDiscoveryPool.GuildInfo info)
        {
            GuildDiscoveryInfo discoveryInfo = (GuildDiscoveryInfo)info.PublicDiscoveryInfo;
            GuildDiscoveryServerOnlyInfo discoveryServerOnlyInfo = (GuildDiscoveryServerOnlyInfo)info.ServerOnlyDiscoveryInfo;

            return Filter(discoveryInfo, discoveryServerOnlyInfo);
        }

        protected override sealed bool TryMakeSpaceFor(IGuildDiscoveryPool.GuildInfo info)
        {
            GuildDiscoveryInfo discoveryInfo = (GuildDiscoveryInfo)info.PublicDiscoveryInfo;
            GuildDiscoveryServerOnlyInfo discoveryServerOnlyInfo = (GuildDiscoveryServerOnlyInfo)info.ServerOnlyDiscoveryInfo;

            return TryMakeSpaceFor(discoveryInfo, discoveryServerOnlyInfo);
        }

        /// <inheritdoc cref="ContextFilter(GuildDiscoveryPlayerContextBase, GuildDiscoveryInfoBase, GuildDiscoveryServerOnlyInfoBase)"/>
        public abstract bool ContextFilter(GuildDiscoveryPlayerContextBase playerContext, GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo);

        /// <inheritdoc cref="Filter(GuildDiscoveryInfoBase, GuildDiscoveryServerOnlyInfoBase)"/>
        public abstract bool Filter(GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo);

        protected virtual bool TryMakeSpaceFor(GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo) => base.TryMakeSpaceFor(new IGuildDiscoveryPool.GuildInfo(publicDiscoveryInfo, serverOnlyDiscoveryInfo));

        #region Migration

        [MetaSerializable]
        public struct Version1GuildDiscoveryGuildData
        {
            [MetaMember(1)] public GuildDiscoveryInfo           PublicDiscoveryInfo;
            [MetaMember(2)] public GuildDiscoveryServerOnlyInfo ServerOnlyDiscoveryInfo;
        };
        [MetaSerializable]
        public struct Version1GuildDiscoveryPoolEntry
        {
            [MetaMember(1)] public Version1GuildDiscoveryGuildData  Info;
            [MetaMember(2)] public MetaTime                         LastRefreshedAt;
        }
        [MetaSerializable]
        public class Version1GuildDiscoveryPoolPage
        {
            [MetaMember(1)] public Version1GuildDiscoveryPoolEntry[] Entries;
        }

        protected override GuildDiscoveryPoolPage ParseLegacyVersion1PoolPage(byte[] payload)
        {
            Version1GuildDiscoveryPoolPage legacyPage = MetaSerialization.DeserializeTagged<Version1GuildDiscoveryPoolPage>(payload, MetaSerializationFlags.Persisted, resolver: null, logicVersion: null);

            GuildDiscoveryPoolPage page = new GuildDiscoveryPoolPage();
            page.Entries = new GuildDiscoveryPoolEntry[legacyPage.Entries.Length];
            for (int ndx = 0; ndx < legacyPage.Entries.Length; ++ndx)
            {
                Version1GuildDiscoveryPoolEntry legacyEntry = legacyPage.Entries[ndx];
                page.Entries[ndx] = new GuildDiscoveryPoolEntry()
                {
                    PublicDiscoveryInfo = legacyEntry.Info.PublicDiscoveryInfo,
                    ServerOnlyDiscoveryInfo = legacyEntry.Info.ServerOnlyDiscoveryInfo,
                    LastRefreshedAt = legacyEntry.LastRefreshedAt,
                };
            }

            return page;
        }

        #endregion
    }

    public class MemberCountGuildPool : GameGuildDiscoveryPool
    {
        int _minSize;
        int _maxSize;

        protected override int MaxNumEntries => 1000;

        public MemberCountGuildPool(string poolId, int minSize = 0, int maxSize = 1000) : base(poolId)
        {
            _minSize = minSize;
            _maxSize = maxSize;
        }

        public override bool Filter(GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo)
        {
            // within the range
            if (publicDiscoveryInfo.NumMembers < _minSize || publicDiscoveryInfo.NumMembers > _maxSize)
                return false;

            // not full
            if (publicDiscoveryInfo.NumMembers >= publicDiscoveryInfo.MaxNumMembers)
                return false;

            // recently has been had a member logged in (within a week). Note that ServerOnlyDiscoveryInfo could be old by itself so we want to be very relaxed here.
            if (MetaTime.Now - serverOnlyDiscoveryInfo.MemberOnlineLatestAt > MetaDuration.FromDays(7))
                return false;

            return true;
        }

        public override bool ContextFilter(GuildDiscoveryPlayerContextBase playerContext, GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo)
        {
            // some pool-specific rules?
            return true;
        }
    }

    public class RecentGuildsPool : GameGuildDiscoveryPool
    {
        protected override int MaxNumEntries => 1;

        public RecentGuildsPool(string poolId) : base(poolId)
        {
        }

        public override bool Filter(GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo)
        {
            // must be younger than 24 hours
            if (MetaTime.Now - serverOnlyDiscoveryInfo.GuildCreatedAt > MetaDuration.FromHours(24))
                return false;

            // not full
            if (publicDiscoveryInfo.NumMembers >= publicDiscoveryInfo.MaxNumMembers)
                return false;

            return true;
        }

        public override bool ContextFilter(GuildDiscoveryPlayerContextBase playerContext, GuildDiscoveryInfo publicDiscoveryInfo, GuildDiscoveryServerOnlyInfo serverOnlyDiscoveryInfo)
        {
            // some pool-specific rules?
            return true;
        }

        protected override bool TryMakeSpaceFor(GuildDiscoveryInfo newPublicDiscoveryInfo, GuildDiscoveryServerOnlyInfo newServerOnlyDiscoveryInfo)
        {
            // Scan whole pool thru, and drop the oldest. Except if the new info is older than any, don't.
            // \todo: better way to do this. We could keep the best/worst time in cache, and update on demand. Or sort the list.

            EntityId oldestGuild = EntityId.None;
            MetaTime oldestTime = MetaTime.Epoch;
            foreach ((var guildId, var entry) in _entries)
            {
                if (oldestGuild == EntityId.None || entry.ServerOnlyDiscoveryInfo.GuildCreatedAt < oldestTime)
                {
                    oldestGuild = guildId;
                    oldestTime = entry.ServerOnlyDiscoveryInfo.GuildCreatedAt;
                }
            }

            // older than any => not updating
            if (newServerOnlyDiscoveryInfo.GuildCreatedAt <= oldestTime)
                return false;

            return _entries.Remove(oldestGuild);
        }
    };

    [EntityConfig]
    public class GuildRecommenderConfig : GuildRecommenderConfigBase
    {
        public override Type EntityActorType => typeof(GuildRecommenderActor);
    }

    public class GuildRecommenderActor : GuildRecommenderActorBase
    {
        IGuildDiscoveryPool _smallGuilds;
        IGuildDiscoveryPool _mediumGuilds;
        IGuildDiscoveryPool _largeGuilds;
        IGuildDiscoveryPool _latestGuilds;

        public GuildRecommenderActor(EntityId entityId) : base(entityId)
        {
            _smallGuilds    = RegisterPool(new MemberCountGuildPool("SmallGuilds", minSize: 1, maxSize: 6));
            _mediumGuilds   = RegisterPool(new MemberCountGuildPool("MediumGuilds", minSize: 7, maxSize: 15));
            _largeGuilds    = RegisterPool(new MemberCountGuildPool("LargeGuilds", minSize: 16));
            _latestGuilds   = RegisterPool(new RecentGuildsPool("RecentGuilds"));
        }

        protected override List<GuildDiscoveryInfoBase> CreateRecommendations(GuildDiscoveryPlayerContextBase playerContextBase)
        {
            // Query 5-10 large, medium and small guilds, and one New guild.

            GuildDiscoveryPlayerContext playerContext = (GuildDiscoveryPlayerContext)playerContextBase;
            GuildRecommendationMixer mixer = new GuildRecommendationMixer();
            mixer.AddSource(_smallGuilds, playerContext, minCount: 5, maxCount: 10);
            mixer.AddSource(_mediumGuilds, playerContext, minCount: 5, maxCount: 10);
            mixer.AddSource(_largeGuilds, playerContext, minCount: 5, maxCount: 10);
            mixer.AddSource(_latestGuilds, playerContext, minCount: 1, maxCount: 1);
            return mixer.Mix(maxCount: 20);
        }
    }
}
