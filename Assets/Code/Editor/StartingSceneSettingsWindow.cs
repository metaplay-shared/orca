using Code.UI.Application;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Orca.Unity.StartingSceneManager {
	internal class StartingSceneSettingsWindow : EditorWindow {
		[MenuItem("Orca/Starting Scene Manager")]
		private static void SelectSettingsInstance() {
			// Get existing open window or if none, make a new one:
			var window = GetWindow<StartingSceneSettingsWindow>(
				"Starting Scene Manager",
				true
			);
			window.Show();
		}

		private void OnGUI() {
			EditorSceneManager.playModeStartScene = (SceneAsset) EditorGUILayout.ObjectField(
				"Starting Scene",
				EditorSceneManager.playModeStartScene,
				typeof(SceneAsset),
				false
			);

			if (GUILayout.Button("Set scene with 0 index")) {
				var first = EditorBuildSettings.scenes[0];
				var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(first.path);
				StaringSceneManager.SetStartingScene(sceneAsset);
			}

			if (GUILayout.Button("Set Active Scene as Starting Scene")) {
				var activeScene = SceneManager.GetActiveScene();
				if (!string.IsNullOrEmpty(activeScene.path)) {
					var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(activeScene.path);
					StaringSceneManager.SetStartingScene(sceneAsset);
				} else {
					Debug.LogError(
						"Can't set active scene as the starting scene as it is an temporary scene. " +
						"If you want to start playing from the active scene you can clear the " +
						"starting scene if it is set."
					);
				}
			}

			if (GUILayout.Button("Clear staring scene")) {
				StaringSceneManager.SetStartingScene(null);
			}
		}
	}
}
