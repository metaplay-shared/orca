// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Unity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.Localization;
using UnityEditor;
using UnityEngine;
using GameConfigBuildIntegration = Metaplay.Core.Config.GameConfigBuildIntegration;

/// <summary>
/// Minimal Game Config building utility for building the StaticGameConfig.mpa (for server to use)
/// and SharedGameConfig.mpa (for client and offline mode to use) config archives.
///
/// This has just the minimal functionality required for the Hello World sample to work.
/// See the Idler reference project for a more comprehensive config builder.
/// </summary>
public static class UnityGameConfigBuilder
{
    public const string SharedGameConfigPath   = "Assets/StreamingAssets/SharedGameConfig.mpa";
    public const string StaticGameConfigPath   = "Backend/Server/GameConfig/StaticGameConfig.mpa";
    const string ServerLocalizationsPath       = "Backend/Server/GameConfig/Localizations.mpa";
    const string UnitTestingSharedGameConfigPath = "Backend/CloudCore.Tests/resources/UnitTesting-SharedGameConfig.mpa";
    const string LocalizationsPath              = "Assets/StreamingAssets/Localizations";               // Directory for persisting localization files (one file per language)
    const string GoogleSheetsCredentialsPath    = "Secrets/orca-sa-identity.json";               // Add path to GCP service account credentials file when configured

    [MenuItem("Config Builder/Build GameConfig/Demo", isValidateFunction: false)]
    public static void BuildPrimaryGameConfig() {
        GameConfigBuildIntegration integration = IntegrationRegistry.Get<GameConfigBuildIntegration>();
        IEnumerable<GameConfigBuildSource> gameConfigBuildSources =
            integration.GetAvailableGameConfigBuildSources(nameof(DefaultGameConfigBuildParameters.DefaultSource)).ToList();
        GoogleSheetBuildSource source = (GoogleSheetBuildSource)gameConfigBuildSources.FirstOrDefault(s => s.DisplayName == "Demo");
        if (source != null) {
            BuildFullGameConfig(source, SharedGameConfigPath, StaticGameConfigPath);
        }
    }

    [MenuItem("Config Builder/Build GameConfig/Develop", isValidateFunction: false)]
    public static void BuildDevelopGameConfig() {
        GameConfigBuildIntegration integration = IntegrationRegistry.Get<GameConfigBuildIntegration>();
        IEnumerable<GameConfigBuildSource> gameConfigBuildSources =
            integration.GetAvailableGameConfigBuildSources(nameof(DefaultGameConfigBuildParameters.DefaultSource)).ToList();
        GoogleSheetBuildSource source = (GoogleSheetBuildSource)gameConfigBuildSources.FirstOrDefault(s => s.DisplayName == "Develop");
        if (source != null) {
            BuildFullGameConfig(source, SharedGameConfigPath, StaticGameConfigPath);
        }
    }

    // [MenuItem("Config Builder/Build Alternative GameConfig", isValidateFunction: false)]
    // public static void BuildAlternativeGameConfig() {
    //     BuildFullGameConfig(AlternativeGoogleSheetId, SharedGameConfigPath, StaticGameConfigPath);
    // }
    //
    // [MenuItem("Config Builder/Build Unit Testing GameConfig", isValidateFunction: false)]
    // public static void BuildUnitTestingGameConfig() {
    //     BuildFullGameConfig(UnitTestingGoogleSheetId, UnitTestingSharedGameConfigPath);
    // }

    public static void BuildFullGameConfig(GoogleSheetBuildSource source, string sharedConfigPath, string staticConfigPath = "")
    {
        // \todo #r19: Should no longer be needed, but keeping here in case needs to be reverted
        // Ensure that types have been registered
        //MetaplayCore.Initialize();
        //MetaplaySDK.InitSerialization();

        // Build full config (Shared + Server)
        OrcaGameConfigBuildParameters buildParams = new OrcaGameConfigBuildParameters();
        buildParams.DefaultSource = source;

        IGameConfigSourceFetcherConfig fetcherConfig = GameConfigSourceFetcherConfigCore.Create().WithGoogleCredentialsFilePath(GoogleSheetsCredentialsPath);

        // Build full config (Shared + Server)
        ConfigArchive fullConfigArchive = Task.Run(async () => await StaticFullGameConfigBuilder.BuildArchiveAsync(MetaTime.Now, parentId: MetaGuid.None, parent: null, buildParams: buildParams, fetcherConfig)).GetAwaiter().GetResult();

        if (!String.IsNullOrEmpty(sharedConfigPath)) {
            // Extract SharedGameConfig from full config & write to disk, for use by the client
            ConfigArchive sharedConfigArchive = ConfigArchive.FromBytes(fullConfigArchive.GetEntryBytes("Shared.mpa"));
            Debug.Log($"Writing {sharedConfigPath} with {sharedConfigArchive.Entries.Count} entries:\n{string.Join("\n", sharedConfigArchive.Entries.Select(entry => $"  {entry.Name} ({entry.Bytes.Length} bytes): {entry.Hash}"))}");
            ConfigArchiveBuildUtility.WriteToFile(sharedConfigPath, sharedConfigArchive);
        }

        if (!String.IsNullOrEmpty(staticConfigPath)) {
            // Write FullGameConfig to disk
            Debug.Log($"Writing {staticConfigPath} with {fullConfigArchive.Entries.Count} entries:\n{string.Join("\n", fullConfigArchive.Entries.Select(entry => $"  {entry.Name} ({entry.Bytes.Length} bytes): {entry.Hash}"))}");
            ConfigArchiveBuildUtility.WriteToFile(staticConfigPath, fullConfigArchive);
        }

        // If in editor, refresh AssetDatabase to make sure Unity sees changed files
        AssetDatabase.Refresh();
    }

    static ConfigArchiveEntry CreateLocalizationArchiveEntry(SpreadsheetContent sheet)
    {
        // Parse sheet to LocalizationLanguage and export in binary-serialized format
        LanguageId              languageId  = LanguageId.FromString(sheet.Name);
        LocalizationLanguage    language    = LocalizationLanguage.FromSpreadsheetContent(languageId, sheet);
        return language.ExportBinaryConfigArchiveEntry();
    }

    [MenuItem("Config Builder/Build Localizations")]
    static void BuildLocalizations()
    {
        GameConfigBuildIntegration integration = IntegrationRegistry.Get<GameConfigBuildIntegration>();
        IEnumerable<GameConfigBuildSource> gameConfigBuildSources =
            integration.GetAvailableGameConfigBuildSources(nameof(DefaultGameConfigBuildParameters.DefaultSource)).ToList();
        GoogleSheetBuildSource source = (GoogleSheetBuildSource)gameConfigBuildSources.FirstOrDefault(s => s.DisplayName == "Demo");
        if (source == null)
        {
            Debug.LogError("No GoogleSheetBuildSource found for 'Demo' in GameConfigBuildIntegration");
            return;
        }
        // Execute in background to avoid deadlocking main thread
        Task.Run(async () =>
        {
            // Fetch 'Localizations' Google sheet
            SpreadsheetContent sheet = await GoogleSheetFetcher.FetchSheetAsync(
                GoogleSheetsCredentialsPath,
                source.SpreadsheetId,
                "Localizations",
                CancellationToken.None
            );
            sheet = sheet.FilterRows((List<string> row, int rowNdx) =>
            {
                // Remove empty rows
                if (row.Count == 0)
                    return false;

                // Remove comment rows
                if (row[0].StartsWith("//", StringComparison.Ordinal))
                    return false;

                return true;
            });
            sheet = sheet.FilterColumns((string colName, int colNdx) => !colName.Contains("Context"));

            // Split input sheet into per-language sheets
            List<SpreadsheetContent> languageSheets = GameConfigHelper.SplitLanguageSheets(sheet, allowMissingTranslations: true);

            // Convert sheets to ConfigArchiveEntries
            ConfigArchiveEntry[] entries = languageSheets
                .Select(sheet => CreateLocalizationArchiveEntry(sheet))
                .ToArray();

            // Create the final ConfigArchive
            MetaTime timestamp = MetaTime.Now;
            ConfigArchive archive = ConfigArchive.CreateFromBuild(timestamp, entries);

            // Write each language file to LocalizationsPath
            Debug.Log($"Writing ConfigArchive as multiple files into {LocalizationsPath} with {archive.Entries.Count} entries:\n{string.Join("\n", archive.Entries.Select(entry => $"  {entry.Name} ({entry.Bytes.Length} bytes): {entry.Hash}"))}");
            await ConfigArchiveBuildUtility.WriteToFileAsync(ServerLocalizationsPath, archive);
            ConfigArchiveBuildUtility.FolderEncoding.WriteToDirectory(archive, LocalizationsPath);
        }).GetAwaiter().GetResult();

        // If in editor, refresh AssetDatabase to make sure Unity sees changed files
        AssetDatabase.Refresh();
    }

    [MenuItem("Config Builder/Build All")]
    static void BuildAll()
    {
        BuildPrimaryGameConfig();
        //BuildUnitTestingGameConfig();
        BuildLocalizations();
    }
}
