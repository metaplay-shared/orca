using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Code.Editor.BuildPipeline {
	public class EditorUserBuildSettingsProcessor : IPreprocessBuildWithReport {
		public int callbackOrder => BuildCallbackOrder.PreprocessBuildCallbackOrder.EditorUserBuildSettings;

		public void OnPreprocessBuild(BuildReport report) {
			bool release = AuxEditorUserBuildSettings.ActiveBuildType == BuildType.Release;

			Debug.Log($"Setting {nameof(EditorUserBuildSettings)} with release={release}");

			EditorUserBuildSettings.buildAppBundle = release;
			EditorUserBuildSettings.development = !release;
			EditorUserBuildSettings.buildAppBundle = release;
			EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
		}
	}
}