// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Game.Server.Database;
using Metaplay.Cloud;
using Metaplay.Cloud.Cluster;
using Metaplay.Cloud.Entity;
using Metaplay.Core;
using Metaplay.Server;
using Metaplay.Server.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Server
{
    /// <summary>
    /// Game-specific server entrypoint.
    /// </summary>
    class ServerMain : ServerMainBase
    {
        public ServerMain()
        {
        }

        Task StartApplicationServiceAsync()
        {
            // \note This is a good place to inject any debug or test code which requires the clustering and entity shards to be set up.

            return Task.CompletedTask;
        }

        Task StopApplicationServiceAsync()
        {
            return Task.CompletedTask;
        }

        protected override void HandleKeyPress(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.K:
                    MetaTime before = MetaTime.Now;
                    MetaTime.DebugTimeOffset += MetaDuration.FromMinutes(10);
                    MetaTime after = MetaTime.Now;
                    Console.WriteLine("Skipping 10min forward from {0} to {1}", before, after);
                    break;

                default:
                    break;
            }
        }

        static async Task<int> Main(string[] cmdLineArgs)
        {
            using (ServerMain program = new ServerMain())
                return await program.RunServerAsync(cmdLineArgs);
        }
    }
}
