// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.GameConfigTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Game.Logic;
using GameConfigBuildIntegration = Metaplay.Core.Config.GameConfigBuildIntegration;

namespace GameConfigTool
{
    class GameConfigTool : GameConfigToolBase
    {
        const string GoogleSheetsCredentials = "Secrets/orca-sa-identity.json"; // Path to Google service account credentials .json

#pragma warning disable CS0169
        // Explicit reference to SharedGameConfig to force linker to include CloudCore in this project
        private static Game.Logic.SharedGameConfig sharedGameConfig;
#pragma warning restore CS0169

        protected override IGameConfigSourceFetcherConfig FetcherConfig => GameConfigSourceFetcherConfigCore.Create()
            .WithGoogleCredentialsFilePath(GoogleSheetsCredentials);

        public async Task<int> RunAsync(string[] args)
        {
            // Default to 'build --dry-run' if no argument given
            if (args.Length == 0)
                args = new string[] { "sources", "--dry-run" };

            // Handle different commands
            string command = args[0];
            if (command == "build")
            {
                bool isDryRun = args.Contains("--dry-run");

                Console.WriteLine("Building StaticGameConfig archive..");
                OrcaGameConfigBuildParameters buildParams = new OrcaGameConfigBuildParameters();
                GameConfigBuildIntegration integration = IntegrationRegistry.Get<GameConfigBuildIntegration>();

                IEnumerable<GameConfigBuildSource> gameConfigBuildSources =
                    integration.GetAvailableGameConfigBuildSources(nameof(buildParams.DefaultSource)).ToList();

                Console.WriteLine($"Available Sources: {string.Join(", ", gameConfigBuildSources.Select(s => s.DisplayName))}");

                bool hasSource = args.Contains("--source");
                if (hasSource)
                {
                    string sourceName = args[Array.IndexOf(args, "--source") + 1];
                    buildParams.DefaultSource = gameConfigBuildSources.FirstOrDefault(s => s.DisplayName == sourceName);
                    if (buildParams.DefaultSource == null)
                    {
                        Console.WriteLine($"Source '{sourceName}' not found!");
                        return 10;
                    }
                }
                else
                {
                    // Defaults to the first configured source
                    buildParams.DefaultSource =
                        integration.GetAvailableGameConfigBuildSources(nameof(buildParams.DefaultSource)).First();
                }

                Console.WriteLine(buildParams.DefaultSource is GoogleSheetBuildSource source
                    ? $"Building from Source: {source.DisplayName} (https://docs.google.com/spreadsheets/d/{source.SpreadsheetId})"
                    : $"Building from Source: {buildParams.DefaultSource.DisplayName}");
                await BuildStaticGameConfigAsync(buildParams, writeOutputFiles: !isDryRun);
            }
            else if (command == "sources")
            {
                Console.WriteLine("Available sources:");
                Console.WriteLine("To build from a specific source, use 'build --source <source>'.");
                GameConfigBuildIntegration integration = IntegrationRegistry.Get<GameConfigBuildIntegration>();
                IEnumerable<GameConfigBuildSource> gameConfigBuildSources =
                    integration.GetAvailableGameConfigBuildSources(nameof(OrcaGameConfigBuildParameters.DefaultSource)).ToList();
                foreach (GameConfigBuildSource source in gameConfigBuildSources)
                {
                    Console.WriteLine(source is GoogleSheetBuildSource
                        ? $"  {source.DisplayName} (https://docs.google.com/spreadsheets/d/{(source as GoogleSheetBuildSource).SpreadsheetId})"
                        : $"  {source.DisplayName}");
                }
            }
            else if (command == "print")
            {
                await PrintGameConfigAsync();
            }
            else if (command == "publish")
            {
                // \todo [petri] support targets other than localhost
                string target = "localhost"; // (args.Length >= 2) ? args[1] : "localhost";

                // Publish StaticGameConfig to target
                Console.WriteLine("Publishing StaticGameConfig to '{0}'..", target);
                await PublishGameConfigAsync("http://localhost:5550/api/", "gameConfig", authorizationToken: null, queryParams: null);
            }
            else
            {
                Console.WriteLine("Invalid command '{0}'!", command);
                return 15;
            }

            return 0;
        }

        static async Task<int> Main(string[] args)
        {
            // Switch to project root directory
            // \todo [petri] This is a hack to get the same project paths to work as are used from within Unity.
            //               Need to figure out a holistic solution for configuring the builds from Unity, this
            //               utility and the dashboard.
            Directory.SetCurrentDirectory("../..");

            // Initialize and run the tool
            GameConfigTool tool = new GameConfigTool();
            return await tool.RunAsync(args);
        }
    }
}
