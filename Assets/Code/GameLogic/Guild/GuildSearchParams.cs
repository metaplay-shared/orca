// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core.Model;
using Metaplay.Core.Guild;

namespace Game.Logic
{
    /// <summary>
    /// The guild search parameters supplied by the client. This class is separated
    /// to a base class, for Metaplay-core fields, and the deriving class, for
    /// game-specific parts. To add new search parameters, add them here.
    /// </summary>
    [MetaSerializableDerived(1)]
    public sealed class GuildSearchParams : GuildSearchParamsBase
    {
        // Example: Add custom search params here
        //  [MetaMember(101)] public int MinNumMembers { get; set; }

        public GuildSearchParams() { }
        public GuildSearchParams(string searchString) : base(searchString)
        {
        }

        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            // Example: we could validate MinNumMembers
            //  if (MinNumMembers < 0)
            //      return false;

            return true;
        }
    }
}
