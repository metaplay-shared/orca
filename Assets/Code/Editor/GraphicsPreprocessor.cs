using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Experimental;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Code.Editor {
	public class GraphicsPreprocessor : AssetPostprocessor {
		private static string GRAPHICS_PATH = "Assets/Graphics/";
		private static string CHAINS_PATH = "Chains/";

		private static void OnPostprocessAllAssets(
			string[] importedAssets,
			string[] deletedAssets,
			string[] movedAssets,
			string[] movedFromAssetPaths
		) {
			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

			AddressableAssetGroup itemChainsGroup = settings.FindGroup("Item Chains");
			if (itemChainsGroup == null) {
				itemChainsGroup = settings.CreateGroup("Item Chains", false, true, true, settings.DefaultGroup.Schemas);
			}

			var chainFolders = importedAssets.Where(
				s => s.Contains(GRAPHICS_PATH + CHAINS_PATH) &&
					(File.GetAttributes(s) & FileAttributes.Directory) == FileAttributes.Directory
			).ToList();
			foreach (var folder in chainFolders) {
				string chain = Path.GetRelativePath(GRAPHICS_PATH + CHAINS_PATH, folder);
				chain = new String(chain.Where(c => !char.IsControl(c)).ToArray());

				string atlasName = chain + ".spriteatlasv2";
				string atlasPath = GRAPHICS_PATH + CHAINS_PATH + atlasName;
				if (!File.Exists(atlasPath)) {
					CreateAtlas(atlasPath, folder);
				}

				string assetGUID = AssetDatabase.AssetPathToGUID(atlasPath);
				AssetReference assetReference = settings.CreateAssetReference(assetGUID);
				assetReference.OperationHandle.WaitForCompletion();

				AddressableAssetEntry assetEntry = settings.FindAssetEntry(assetGUID);
				assetEntry.labels.Add("Chains");
				assetEntry.SetAddress($"Chains/{chain}");

				settings.MoveEntry(assetEntry, itemChainsGroup, true);
			}

			AssetDatabase.Refresh();
		}

		private static void CreateAtlas(string atlasPath, string assetPath) {
			var asset = new SpriteAtlasAsset();

			var packing = new SpriteAtlasPackingSettings();
			packing.enableTightPacking = false;
			packing.enableRotation = false;
			packing.padding = 2;

			asset.SetPackingSettings(packing);

			var dir = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
			Debug.Log(dir);

			asset.Add(new[] { dir });

			Debug.Log("Saving to " + atlasPath);
			SpriteAtlasAsset.Save(asset, atlasPath);
		}
	}
}
