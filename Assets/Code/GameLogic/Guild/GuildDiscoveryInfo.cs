// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.GuildDiscovery;
using Metaplay.Core.Model;

namespace Game.Logic
{
    /// <summary>
    /// <inheritdoc cref="GuildDiscoveryInfoBase"/>
    /// </summary>
    [MetaSerializableDerived(1)]
    public sealed class GuildDiscoveryInfo : GuildDiscoveryInfoBase
    {
        // Add game-specific fields here

        //[MetaMember(101)] public int    CoolnessIndex;

        public GuildDiscoveryInfo() { }
        public GuildDiscoveryInfo(EntityId guildId, string displayName, int numMembers, int maxNumMembers) : base(guildId, displayName, numMembers, maxNumMembers)
        {
        }
    }
}
