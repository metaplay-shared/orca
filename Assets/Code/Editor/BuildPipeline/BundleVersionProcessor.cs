using Orca.Utilities.BuildPipeline.Editor;
using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Code.Editor.BuildPipeline {
	public class BundleVersionProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport {
		private string iOSOriginalBuildNumber;
		private int androidOriginalBundleVersionCode;
		public int callbackOrder => BuildCallbackOrder.PreprocessBuildCallbackOrder.BundleVersion;

		public void OnPreprocessBuild(BuildReport report) {
			int.TryParse(CommandLineArguments.GetCommandLineArg("-buildCounter"), out int buildCounter);
			// Build will fail if counter is zero
			int bundleVersion = Math.Max(buildCounter, 1);
			Debug.Log($"Resolved bundle version as '{bundleVersion.ToString()}'");
			BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
			switch (activeBuildTarget) {
				case BuildTarget.Android:
					androidOriginalBundleVersionCode = PlayerSettings.Android.bundleVersionCode;
					PlayerSettings.Android.bundleVersionCode = bundleVersion;
					break;
				case BuildTarget.iOS:
					iOSOriginalBuildNumber = PlayerSettings.iOS.buildNumber;
					PlayerSettings.iOS.buildNumber = bundleVersion.ToString();
					break;
				case BuildTarget.WebGL:

					break;
				default:
					Debug.LogError(
						$"{nameof(BundleVersionProcessor)}: Un-handled build target '{activeBuildTarget.ToString()}'"
					);
					break;
			}
		}

		public void OnPostprocessBuild(BuildReport report) {
			BuildTarget activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
			switch (activeBuildTarget) {
				case BuildTarget.Android:
					PlayerSettings.Android.bundleVersionCode = androidOriginalBundleVersionCode;
					break;
				case BuildTarget.iOS:
					PlayerSettings.iOS.buildNumber = iOSOriginalBuildNumber;
					break;
				case BuildTarget.WebGL:

					break;
				default:
					Debug.LogError(
						$"{nameof(BundleVersionProcessor)}: Un-handled build target '{activeBuildTarget.ToString()}'"
					);
					break;
			}
		}
	}
}