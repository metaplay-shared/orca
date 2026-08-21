// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.Model;
using Metaplay.Server.GuildDiscovery;

namespace Game.Server.GuildDiscovery
{
    [MetaSerializableDerived(1)]
    public sealed class GuildDiscoveryServerOnlyInfo : GuildDiscoveryServerOnlyInfoBase
    {
        // Add any private data needed in recommendation and search.

        //[MetaMember(101)] public int  SecretCoolnessIndex;

        public GuildDiscoveryServerOnlyInfo() { }
        public GuildDiscoveryServerOnlyInfo(MetaTime guildCreatedAt, MetaTime memberOnlineLatestAt) : base(guildCreatedAt, memberOnlineLatestAt)
        {
        }
    }
}
