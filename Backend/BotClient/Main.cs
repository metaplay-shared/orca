// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using System.Threading.Tasks;

namespace Metaplay.BotClient
{
    /// <summary>
    /// Provides a Main method for BotClient application. This file should not be modified.
    /// </summary>
    public static class Launcher
    {
        static async Task<int> Main(string[] cmdLineArgs)
        {
            using (BotClientMain program = new BotClientMain())
                return await program.RunBotsAsync(cmdLineArgs);
        }
    }
}
