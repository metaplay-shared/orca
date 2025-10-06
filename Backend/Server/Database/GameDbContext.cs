// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Cloud.Persistence;
using Metaplay.Server.Database;

namespace Game.Server.Database
{
    /// <summary>
    /// Game-specific EFCore database context. Used to declare the database tables.
    /// </summary>
    public class GameDbContext : MetaDbContext
    {
        // Example table declaration:
        //public DbSet<MyPersistedThing> Things { get; set; }
    }
}
