#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace Code.Editor.BuildPipeline {
	public class IOSExemptEncryptionProcessor : IPostprocessBuildWithReport {
		public int callbackOrder => 0;

		public void OnPostprocessBuild(BuildReport report) {
			Execute(report.summary.platform, report.summary.outputPath);
		}

		private void Execute(BuildTarget platform, string path) {
			if (platform != BuildTarget.iOS) {
				return;
			}

			// Get plist
			string plistPath = path + "/Info.plist";
			PlistDocument plist = new();
			plist.ReadFromString(File.ReadAllText(plistPath));

			// Get root
			PlistElementDict rootDict = plist.root;

			rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

			// Write to file
			File.WriteAllText(plistPath, plist.WriteToString());
		}
	}
}

#endif
