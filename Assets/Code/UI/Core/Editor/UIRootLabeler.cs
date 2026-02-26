using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Code.UI.Core.Editor {
	public class UIRootLabeler : AssetPostprocessor {
		private const string UI_ROOT_LABEL = "UIRoot";

		private static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths
		) {
			foreach (var importedAsset in importedAssets) {
				var asset = AssetDatabase.LoadAssetAtPath<Object>(importedAsset);
				var isUIRootPrefab = IsAssetUIRoot(asset);
				var hasLabel = HasLabel(UI_ROOT_LABEL, asset);
				if (isUIRootPrefab && !hasLabel) {
					AddLabel(UI_ROOT_LABEL, asset);
				} else if (!isUIRootPrefab && hasLabel) {
					RemoveLabel(UI_ROOT_LABEL, asset);
				}
			}
		}

		private static bool IsAssetUIRoot(Object asset) {
			var go = asset as GameObject;
			if (go == null) {
				return false;
			}

			var isUIRoot = go.GetComponent<IUIStackItem>() != null;
			return isUIRoot;
		}

		private static bool HasLabel(string label, Object asset) {
			var labels = AssetDatabase.GetLabels(asset);
			return labels.Contains(label);
		}

		private static void AddLabel(string label, Object asset) {
			Debug.Log($"Add '{label}' label to: {AssetDatabase.GetAssetPath(asset)}");
			var labels = AssetDatabase.GetLabels(asset);
			ArrayUtility.Add(ref labels, label);
			AssetDatabase.SetLabels(asset, labels.Distinct().ToArray());
			EditorUtility.SetDirty(asset);
		}

		private static void RemoveLabel(string label, Object asset) {
			Debug.Log($"Remove '{label}' label from: {AssetDatabase.GetAssetPath(asset)}");
			var labels = AssetDatabase.GetLabels(asset).ToList();
			labels.RemoveAll(l => l == label);
			AssetDatabase.SetLabels(asset, labels.ToArray());
			EditorUtility.SetDirty(asset);
		}
	}
}
