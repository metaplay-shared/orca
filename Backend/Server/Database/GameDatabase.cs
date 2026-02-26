// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Cloud.Persistence;
using Metaplay.Server.Database;

namespace Game.Server.Database
{
    public class GameDatabase : MetaDatabase
    {
        public GameDatabase()
        {

        }

        public static new GameDatabase Get(QueryPriority priority)
        {
            return MetaDatabase.Get<GameDatabase>(priority);
        }

        // Add game-specific database queries here.
    }
}
