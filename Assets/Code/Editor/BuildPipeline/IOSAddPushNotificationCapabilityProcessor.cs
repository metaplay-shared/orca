#if UNITY_IOS
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

namespace Code.Editor.BuildPipeline {
	public class IOSAddPushNotificationCapabilityProcessor : IPostprocessBuildWithReport {
		public int callbackOrder => int.MaxValue;

		public void OnPostprocessBuild(BuildReport report) {
			BuildTarget platform = report.summary.platform;
			if (platform != BuildTarget.iOS) {
				return;
			}

			string buildPath = report.summary.outputPath;
			string projPath = PBXProject.GetPBXProjectPath(buildPath);

			ProjectCapabilityManager capabilityManager = new(
				projPath,
				"Unity-iPhone.entitlements",
				targetName: "Unity-iPhone"
			);
			capabilityManager.AddPushNotifications(false);
			capabilityManager.WriteToFile();
		}
	}
}
#endif
