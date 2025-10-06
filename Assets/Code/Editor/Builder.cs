using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metaplay.Core;
using Metaplay.Core.Config;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Player;
using UnityEngine;

namespace Code.Editor {
	public static class Builder {
		private static readonly string[] Scenes = {
			"Assets/Scenes/Start.unity", "Assets/Scenes/Loading.unity", "Assets/Scenes/Game.unity"
		};

		[MenuItem("Orca/CI/Build Android dev")]
		public static void BuildAndroidCI() {
			//EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
			BuildConfig();
			BuildAddressables();

			var release = HasArg("-release");
			var exportGradleProject = HasArg("-exportGradleProject");

			EditorUserBuildSettings.buildAppBundle = release;
			EditorUserBuildSettings.development = !release;
			EditorUserBuildSettings.exportAsGoogleAndroidProject = exportGradleProject;

			try {
				int.TryParse(GetCommandLineArg("-buildCounter"), out var buildCounter);

				// Build will fail if counter is zero
				PlayerSettings.Android.bundleVersionCode = Math.Max(buildCounter, 1);
			} catch (Exception) { }

			EditorUserBuildSettings.buildAppBundle = release;

			EditorUserBuildSettings.androidCreateSymbols =
				release ? AndroidCreateSymbols.Public : AndroidCreateSymbols.Debugging;

			UnityEditor.BuildPipeline.BuildPlayer(
				Scenes,
				release ? "Build/orca.aab" : "Build/orca.apk",
				BuildTarget.Android,
				release ? BuildOptions.None : BuildOptions.Development
			);
		}

		[MenuItem("Orca/CI/Build WebGL")]
		public static void BuildWebGLCI() {
			bool configSourceArg = HasArg("-configSource");
			GoogleSheetBuildSource configSource = null;
			if (configSourceArg) {
				string configSourceValue = GetCommandLineArg("-configSource");
				configSource = GetConfigSource(configSourceValue);

				Debug.Log($"<<Builder>> Config source: {configSourceValue}, args: {string.Join(", ", System.Environment.GetCommandLineArgs())}");
			} else if (Environment.GetEnvironmentVariable("METAPLAY_CONFIG_SOURCE") != null) {
				string configSourceValue = Environment.GetEnvironmentVariable("METAPLAY_CONFIG_SOURCE");
				configSource = GetConfigSource(configSourceValue);
			} else {
				Debug.Log("<<Builder>> No config source specified, args: " + string.Join(", ", System.Environment.GetCommandLineArgs()));
			}

			BuildConfig(configSource);
            BuildAddressables();

			UnityEditor.BuildPipeline.BuildPlayer(
				Scenes,
				"Build/WebGL",
				BuildTarget.WebGL,
				BuildOptions.CleanBuildCache
			);
		}

		#if UNITY_CLOUD_BUILD
        public static void PreExport(UnityEngine.CloudBuild.BuildManifestObject manifest) {
			int buildNumber = manifest.GetValue<int>("buildNumber");
            PlayerSettings.bundleVersion = $"0.1.{buildNumber}";
			PlayerSettings.iOS.buildNumber = $"{buildNumber}";
			PreiOSCI();
        }
		#endif

		public static void PreiOSCI() {
			bool configSourceArg = HasArg("-configSource");
			GoogleSheetBuildSource configSource = null;
			if (configSourceArg) {
				string configSourceValue = GetCommandLineArg("-configSource");
				configSource = GetConfigSource(configSourceValue);
			} else if (Environment.GetEnvironmentVariable("METAPLAY_CONFIG_SOURCE") != null) {
				string configSourceValue = Environment.GetEnvironmentVariable("METAPLAY_CONFIG_SOURCE");
				configSource = GetConfigSource(configSourceValue);
			}

			BuildConfig(configSource);
			BuildAddressables();
		}

		[MenuItem("Orca/CI/Build iOS")]
		public static void BuildiOSCI()
		{
			PreiOSCI();

			UnityEditor.BuildPipeline.BuildPlayer(
				Scenes,
				"Build/iOS",
				BuildTarget.iOS,
				BuildOptions.CleanBuildCache
			);
		}

		[MenuItem("Orca/CI/Build Addressables")]
		public static void BuildAddressables() {
			AddressableAssetSettings.BuildPlayerContent(out var result);

			if (!string.IsNullOrEmpty(result.Error)) {
				throw new Exception("Addressable build failed: " + result.Error);
			}

			AssetDatabase.Refresh();
		}

		private static bool HasArg(string key) {
			string[] args = System.Environment.GetCommandLineArgs();
			return args.Contains(key);
		}

		private static string GetCommandLineArg(string name) {
			var args = System.Environment.GetCommandLineArgs();
			for (var i = 0; i < args.Length; i++) {
				if (args[i] == name &&
					args.Length > i + 1
					) {
					return args[i + 1];
				}
			}

			return string.Empty;
		}

		private static GoogleSheetBuildSource GetConfigSource(string sourceName = "Demo") {
			GameConfigBuildIntegration integration = IntegrationRegistry.Get<GameConfigBuildIntegration>();
			IEnumerable<GameConfigBuildSource> gameConfigBuildSources =
				integration.GetAvailableGameConfigBuildSources(
					nameof(DefaultGameConfigBuildParameters.DefaultSource)
				).ToList();
			return (GoogleSheetBuildSource)gameConfigBuildSources.FirstOrDefault(s => s.DisplayName == sourceName);
		}

		private static void BuildConfig(GoogleSheetBuildSource configSource = null) {
			if (configSource == null) {
				configSource = GetConfigSource();
			}

			if (configSource != null) {
				UnityGameConfigBuilder.BuildFullGameConfig(
					configSource,
					UnityGameConfigBuilder.SharedGameConfigPath,
					UnityGameConfigBuilder.StaticGameConfigPath
				);
			}
		}
	}
}
