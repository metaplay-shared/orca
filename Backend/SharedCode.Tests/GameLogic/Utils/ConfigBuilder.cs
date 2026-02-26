using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Game.Logic;
using NUnit.Framework;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Localization;

namespace CloudCore.Tests.GameLogic.Utils {
	/// <summary>
	/// <c>ConfigBuilder</c> is a utility that allows easy building of game configs and localizations
	/// using command line.
	/// <example>
	/// For example:
	/// <code>
	/// > cd Project-Orca/Backend/CloudCore.Tests
	/// > ORCA_UNIT_TESTING_CONFIG_BUILD_ENABLED=true dotnet test \
	///   --filter "FullyQualifiedName~GameLogic.Utils.ConfigBuilder.BuildUnitTestingGameConfig"
	/// </code>
	/// builds the unit test game config.
	/// </example>
	///
	/// TODO: Remove the code duplication i.e. the functionality was simply copy-pasted from UnityGameConfigBuilder.cs.
	/// </summary>
	[TestFixture]
	public class ConfigBuilder {
		private const string SharedGameConfigPath            = "Assets/StreamingAssets/SharedGameConfig.mpa";
		private const string StaticGameConfigPath            = "Backend/Server/GameConfig/StaticGameConfig.mpa";
		private const string LocalizationsPath               = "Assets/StreamingAssets/Localizations";
		private const string GoogleSheetsCredentialsPath     = "Secrets/orca-sa-identity.json";                                  // Add path to GCP service account credentials file when configured
		private const string PrimaryGoogleSheetId            = "1ZY7vQWsxOiatES0Zr26HzuR652iNbEUH";                       // Identifier of Google Sheet where to fetch source data from

		// Environment variable to be set to 'true' to enable config building. The environment variable
		// prevents accidental building of the config when running unit tests.
		private const string EnableEnvironmentVariable = "ORCA_UNIT_TESTING_CONFIG_BUILD_ENABLED";
		private const string SheetIdEnvironmentVariable = "GOOGLE_SHEET_ID";
		private const string OutputDirEnvironmentVariable = "GAME_CONFIG_OUTPUT_DIR";

		[Test]
		public async Task BuildPrimaryGameConfig() {
			await BuildFullGameConfig(
				PrimaryGoogleSheetId,
				$"{CommonUtils.OrcaProjectRootDir}/{SharedGameConfigPath}",
				$"{CommonUtils.OrcaProjectRootDir}/{StaticGameConfigPath}"
			);
		}

		private async Task BuildFullGameConfig(string sheetId, string sharedConfigPath, string staticConfigPath = "") {
			if (!BuildEnabled()) {
				return;
			}
			// Build full config (Shared + Server)
			DefaultGameConfigBuildParameters buildParams = new DefaultGameConfigBuildParameters();
			buildParams.DefaultSource = new GoogleSheetBuildSource("Default", sheetId);

			IGameConfigSourceFetcherConfig fetcherConfig = GameConfigSourceFetcherConfigCore.Create().WithGoogleCredentialsFilePath($"{CommonUtils.OrcaProjectRootDir}/{GoogleSheetsCredentialsPath}");
			ConfigArchive fullConfigArchive = await StaticFullGameConfigBuilder.BuildArchiveAsync(
				MetaTime.Now,
				parentId: MetaGuid.None,
				parent: null,
				buildParams: buildParams,
				fetcherConfig
			);

			if (!String.IsNullOrEmpty(sharedConfigPath)) {
				// Extract SharedGameConfig from full config & write to disk, for use by the client
				ConfigArchive sharedConfigArchive = ConfigArchive.FromBytes(fullConfigArchive.GetEntryBytes("Shared.mpa"));
				await Console.Out.WriteLineAsync($"Writing {sharedConfigPath} with {sharedConfigArchive.Entries.Count} entries:\n{string.Join("\n", sharedConfigArchive.Entries.Select(entry => $"  {entry.Name} ({entry.Bytes.Length} bytes): {entry.Hash}"))}");
				await ConfigArchiveBuildUtility.WriteToFileAsync(sharedConfigPath, sharedConfigArchive);
			}

			if (!String.IsNullOrEmpty(staticConfigPath)) {
				// Write FullGameConfig to disk
				await Console.Out.WriteLineAsync($"Writing {staticConfigPath} with {fullConfigArchive.Entries.Count} entries:\n{string.Join("\n", fullConfigArchive.Entries.Select(entry => $"  {entry.Name} ({entry.Bytes.Length} bytes): {entry.Hash}"))}");
				await ConfigArchiveBuildUtility.WriteToFileAsync(staticConfigPath, fullConfigArchive);
			}
		}

		[Test]
		public void BuildLocalizations() {
			if (!BuildEnabled()) {
				return;
			}

			string sheetId = GetSheetIdFromEnvironment();
			SpreadsheetContent sheet = FetchLocalization(sheetId);
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
			Console.Out.WriteLine(
				$"Writing ConfigArchive as multiple files into {CommonUtils.OrcaProjectRootDir}/{LocalizationsPath} with {archive.Entries.Count} entries:\n{string.Join("\n", archive.Entries.Select(entry => $"  {entry.Name} ({entry.Bytes.Length} bytes): {entry.Hash}"))}"
			);
			ConfigArchiveBuildUtility.FolderEncoding.WriteToDirectory(archive, $"{CommonUtils.OrcaProjectRootDir}/{LocalizationsPath}");
		}

		[Test]
		public void PrintLocalizations() {
			if (!BuildEnabled()) {
				return;
			}

			string sheetId = GetSheetIdFromEnvironment();
			string resolvedSheetId = ResolveSheetId(sheetId);
			string outputDir = GetOutputDirFromEnvironment();
			SpreadsheetContent sheet = FetchLocalization(resolvedSheetId);
			List<string> lines = new List<string>();
			foreach (List<SpreadsheetCell> row in sheet.Cells) {
				lines.Add(string.Join('\t', row.Select(cell => cell.Value)));
			}

			Task.Run(async () => await File.WriteAllLinesAsync($"{outputDir}/{sheetId}", lines));
		}

		private SpreadsheetContent FetchLocalization(string sheetId) {
			SpreadsheetContent sheet = Task
				.Run(
					async () => await GoogleSheetFetcher.FetchSheetAsync($"{CommonUtils.OrcaProjectRootDir}/{GoogleSheetsCredentialsPath}", sheetId, "Localizations", CancellationToken.None)
				).GetAwaiter().GetResult();

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
			return sheet;
		}

		private ConfigArchiveEntry CreateLocalizationArchiveEntry(SpreadsheetContent sheet)
		{
			// Parse sheet to LocalizationLanguage and export in binary-serialized format
			LanguageId              languageId  = LanguageId.FromString(sheet.Name);
			LocalizationLanguage    language    = LocalizationLanguage.FromSpreadsheetContent(languageId, sheet);
			return language.ExportBinaryConfigArchiveEntry();
		}

		private string GetSheetIdFromEnvironment() {
			string sheetId = Environment.GetEnvironmentVariable(SheetIdEnvironmentVariable);
			Assert.NotNull(sheetId);
			Assert.True(sheetId.Length > 0);
			return sheetId;
		}

		private string ResolveSheetId(string sheetId) {
			Dictionary<string, string> favoriteSheets = new Dictionary<string, string> {
				{ "primary", PrimaryGoogleSheetId },
			};
			return favoriteSheets.ContainsKey(sheetId) ? favoriteSheets[sheetId] : sheetId;
		}

		private string GetOutputDirFromEnvironment() {
			string outputDir = Environment.GetEnvironmentVariable(OutputDirEnvironmentVariable);
			Assert.NotNull(outputDir);
			Assert.True(outputDir.Length > 0);
			return outputDir;
		}

		private bool BuildEnabled() {
			string enabled = Environment.GetEnvironmentVariable(EnableEnvironmentVariable);
			return enabled != null && enabled == "true";
		}
	}
}
