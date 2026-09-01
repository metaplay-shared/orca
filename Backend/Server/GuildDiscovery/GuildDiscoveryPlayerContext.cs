// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core.Model;
using Metaplay.Server.GuildDiscovery;

namespace Game.Server.GuildDiscovery
{
    /// <summary>
    /// The player context in which the discovery operations are made. This might
    /// for example contain the level of the player allow it to be taken into the
    /// account when recommending guilds.
    /// <para>
    /// Created by <see cref="Metaplay.Server.PlayerActorBase.CreateGuildDiscoveryContext"/>.
    /// Consumed by <see cref="GuildSearchActor"/> and <see cref="GuildRecommenderActor"/>.
    /// </para>
    /// </summary>
    [MetaSerializableDerived(1)]
    public class GuildDiscoveryPlayerContext : GuildDiscoveryPlayerContextBase
    {
        // For example, current player level, last known geo-location, and/or language
    }
}
