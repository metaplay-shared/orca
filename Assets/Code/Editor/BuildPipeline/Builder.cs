using JetBrains.Annotations;
using Orca.Utilities.BuildPipeline.Editor;
using System.Linq;
using UnityEditor;

namespace Code.Editor.BuildPipeline {
	public static class Builder {
		[UsedImplicitly]
		public static void BuildPlayer() {
			BuildPlayerOptions options = new () {
				scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
				locationPathName = CommandLineArguments.GetCommandLineArg("-locationPathName"),
				targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup,
				target = EditorUserBuildSettings.activeBuildTarget,
				options = CommandLineArguments.HasFlag("-release")
					? BuildOptions.None
					: BuildOptions.Development
			};
			UnityEditor.BuildPipeline.BuildPlayer(options);
		}
	}
}