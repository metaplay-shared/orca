// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Cloud.Entity;
using Metaplay.Core;
using Metaplay.Core.Guild;
using Metaplay.Core.GuildDiscovery;
using Metaplay.Server.GuildDiscovery;
using System;

namespace Game.Server.GuildDiscovery
{
    [EntityConfig]
    public class GuildSearchConfig : GuildSearchConfigBase
    {
        public override Type EntityActorType => typeof(GuildSearchActor);
    }

    /// <summary>
    /// Game-specific component responsible for the guild search.
    /// </summary>
    public class GuildSearchActor : GuildSearchActorBase
    {
        public GuildSearchActor(EntityId entityId) : base(entityId)
        {
        }

        protected override bool FilterSearchResult(GuildDiscoveryInfoBase publicDiscoveryInfoBase, GuildDiscoveryServerOnlyInfoBase serverOnlyDiscoveryInfoBase, GuildSearchParamsBase searchParams, GuildDiscoveryPlayerContextBase searchContext)
        {
            // Check the name.
            if (!publicDiscoveryInfoBase.DisplayName.Contains(searchParams.SearchString, StringComparison.OrdinalIgnoreCase))
                return false;

            // \todo: add custom filters here

            return true;
        }
    }
}
