using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Orca.Unity.StartingSceneManager {
	internal static class StaringSceneManager {
		private const string StartingSceneKey =
			"com.orca.startingscenemanager-startingSceneAssetPath-orca";

		[InitializeOnLoadMethod]
		private static void Initialize() {
			Debug.Log("Initializing StatingSceneManager");
			var path = Deserialize();
			Debug.Log(path);
			if (string.IsNullOrEmpty(path)) {
				return;
			}

			var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
			SetStartingScene(sceneAsset, false);
		}

		internal static void SetStartingScene(SceneAsset sceneAsset, bool save = true) {
			Debug.Log(
				$"Set staring scene as: {(sceneAsset != null ? sceneAsset.name : "None")}\n{AssetDatabase.GetAssetPath(sceneAsset)}",
				sceneAsset
			);
			EditorSceneManager.playModeStartScene = sceneAsset;
			Serialize(sceneAsset);
		}

		private static void Serialize(SceneAsset sceneAsset) {
			var path = sceneAsset != null
				? AssetDatabase.GetAssetPath(sceneAsset)
				: string.Empty;
			EditorPrefs.SetString(StartingSceneKey, path);
		}

		private static string Deserialize() {
			return EditorPrefs.GetString(StartingSceneKey);
		}
	}
}
