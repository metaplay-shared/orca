using Orca.Utilities.BuildPipeline.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Code.Editor.BuildPipeline {
	public class ActiveBuildTypeProcessor : IPreprocessBuildWithReport {
		public int callbackOrder => BuildCallbackOrder.PreprocessBuildCallbackOrder.ActiveBuildType;

		public void OnPreprocessBuild(BuildReport report) {
			bool release = !Application.isBatchMode
				? EditorUtility.DisplayDialog("Select Build Type", "", "release", "development")
				: CommandLineArguments.HasFlag("-release");
			BuildType activeBuildType = release ? BuildType.Release : BuildType.Development;
			AuxEditorUserBuildSettings.ActiveBuildType = activeBuildType;
			Debug.Log(
				$"{nameof(ActiveBuildTypeProcessor)}: Set active build type as '{activeBuildType.ToString()}'"
			);
		}
	}
}