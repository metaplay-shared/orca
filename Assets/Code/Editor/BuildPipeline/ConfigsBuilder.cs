using System.Collections.Generic;
using System.Linq;
using Metaplay.Core;
using Metaplay.Core.Config;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Code.Editor.BuildPipeline {
	public class ConfigsBuilder : IPreprocessBuildWithReport {
		public int callbackOrder => BuildCallbackOrder.PreprocessBuildCallbackOrder.Configs;

		public void OnPreprocessBuild(BuildReport report) {
			Debug.Log("Building Game Configs");
			BuildConfig();
		}

		private static void BuildConfig() {
			GameConfigBuildIntegration integration = IntegrationRegistry.Get<GameConfigBuildIntegration>();
			IEnumerable<GameConfigBuildSource> gameConfigBuildSources =
				integration.GetAvailableGameConfigBuildSources(nameof(DefaultGameConfigBuildParameters.DefaultSource)).ToList();
			GoogleSheetBuildSource source = (GoogleSheetBuildSource)gameConfigBuildSources.First();
			if (source != null) {
				UnityGameConfigBuilder.BuildFullGameConfig(
					source,
					UnityGameConfigBuilder.SharedGameConfigPath,
					UnityGameConfigBuilder.StaticGameConfigPath
				);
			}
		}
	}
}