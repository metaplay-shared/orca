#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Orca.Unity.BuildPipeline.iOS {
	public class iOSFirebaseAnalyticsPostProcessing : IPostprocessBuildWithReport {
		/// <summary>
		/// Identifier in Info.Plist for Firebase' analytics collection initial status.
		/// Overriding this value in runtime with Analytics.setAnalyticsCollectionEnabled(bool)
		/// will override the value. The value set will remain until calling the previously
		/// mentioned function is called again.
		/// </summary>
		private const string FIR_COLLECTION_ENABLED_KEY = "FIREBASE_ANALYTICS_COLLECTION_ENABLED";
		/// <summary>
		/// Default value to set in Info.plist
		/// </summary>
		private const bool FIL_COLLECTION_ENABLED_VALUE = false;

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

			Debug.Log("Postprocessing Firebase Analytics to Info.Plist");
			Debug.Log($"Setting \"{FIR_COLLECTION_ENABLED_KEY}\" to {FIR_COLLECTION_ENABLED_KEY}");

			// Get plist
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new PlistDocument();
			plist.ReadFromString(File.ReadAllText(plistPath));

			// Get root
			PlistElementDict rootDict = plist.root;

			rootDict.SetBoolean(FIR_COLLECTION_ENABLED_KEY, FIL_COLLECTION_ENABLED_VALUE);

			// Write to file
			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}
#endif