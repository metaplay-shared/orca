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
			// The replacement API, UnityEditor.Android.UserBuildSettings.DebugSymbols.level,
			// ships with the Android editor module, which WebGL-only editors do not have.
			#pragma warning disable CS0618 // Type or member is obsolete
			EditorUserBuildSettings.androidCreateSymbols = AndroidCreateSymbols.Debugging;
			#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}