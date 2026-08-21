#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Orca.Unity.BuildPipeline.iOS {
	public class iOSATTPostProcessing : IPostprocessBuildWithReport {
		private const string USER_TACKING_INFO_KEY = "NSUserTrackingUsageDescription";
		private const string USER_TRACKING_USAGE_DESCRIPTION =
			"This identifier will be used to deliver personalized ads to you.";

		public int callbackOrder => 999;

		public void OnPostprocessBuild(BuildReport report) {
			OnPostprocessBuild(report.summary.platform, report.summary.outputPath);
		}

		public void OnPostprocessBuild(
			BuildTarget buildTarget,
			string path
		) {
			if (buildTarget != BuildTarget.iOS) {
				return;
			}

			Debug.Log("Postprocessing ATT values to Info.Plist");

			// Get plist
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			// Get root
			PlistElementDict rootDict = plist.root;

			rootDict.SetString(USER_TACKING_INFO_KEY, USER_TRACKING_USAGE_DESCRIPTION);

			// Write to file
			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif