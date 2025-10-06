#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Code.Editor.BuildScripts.iOS {
	public class GoogleServiceIOSPostProcess : IPostprocessBuildWithReport {
		public int callbackOrder => 0;

		public void OnPostprocessBuild(BuildReport report) {
			var projectPath = report.summary.outputPath;
			if (report.summary.platform == BuildTarget.iOS) {
				CopyGoogleServicePlist(projectPath);
			}
		}

		private void CopyGoogleServicePlist(string projectPath) {
			// const string fileName = "GoogleService-Info.plist";
			// var src = Path.Combine(Application.dataPath, fileName);
			// var dst = Path.Combine(projectPath, fileName);
			// FileUtil.ReplaceFile(src, dst);
		}

		// https://github.com/firebase/quickstart-unity/issues/862#issuecomment-752945417
		// https://github.com/firebase/quickstart-unity/issues/862#issuecomment-771546659
		[PostProcessBuild]
		public static void OnPostProcessBuildAddFirebaseFile(BuildTarget buildTarget, string pathToBuiltProject) {
			// if (buildTarget == BuildTarget.iOS) {
			// 	// Go get pbxproj file
			// 	string projPath = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";

			// 	// PBXProject class represents a project build settings file,
			// 	// here is how to read that in.
			// 	PBXProject proj = new PBXProject();
			// 	proj.ReadFromFile(projPath);

			// 	// Copy plist from the project folder to the build folder
			// 	proj.AddFileToBuild(
			// 		proj.GetUnityMainTargetGuid(),
			// 		proj.AddFile("GoogleService-Info.plist", "GoogleService-Info.plist")
			// 	);
			// 	// var targetGuid = proj.GetUnityMainTargetGuid();
			// 	// string googleInfoPlistGuid = proj.FindFileGuidByProjectPath("GoogleService-Info.plist");
			// 	// proj.AddFileToBuild(targetGuid, googleInfoPlistGuid);

			// 	// Write PBXProject object back to the file
			// 	proj.WriteToFile(projPath);
			// }
		}
	}
}
#endif