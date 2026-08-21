using System.IO;
using System.Linq;
using System.Text;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Unity;
using UnityEditor;
using UnityEngine;

namespace Code.Editor {
	public class MergeBoardEditor : EditorWindow {
		private int selectedX = 0;
		private int selectedY = 0;
		private int selectedIsland = 0;

		private string island = "MainIsland";
		private string type = "";
		private string level = "1";

		private static SharedGameConfig gameConfig;
		private static MergeBoardModel board;
		private static string[] buttonTexts;
		private static string[] islandTexts;

		[MenuItem("Orca/Board editor")]
		public static void ShowBoardEditor() {
			gameConfig = LoadGameConfig();
			board = new MergeBoardModel(
				gameConfig,
				IslandTypeId.MainIsland,
				MetaTime.Now,
				_ => { }
			);
			buttonTexts = BuildButtons();
			islandTexts = new string[gameConfig.Islands.Count];
			int i = 0;
			foreach (var islandId in gameConfig.Islands.Keys) {
				islandTexts[i] = islandId.ToString();
				i++;
			}
			var window = GetWindow<MergeBoardEditor>(
				"Board editor",
				true
			);
			window.Show();
		}

		private void OnGUI() {
			if (islandTexts == null) {
				return;
			}

			EditorGUILayout.BeginHorizontal();
			selectedIsland = EditorGUILayout.Popup(selectedIsland, islandTexts);
			island = islandTexts[selectedIsland];
			if (GUILayout.Button("Refresh")) {
				IslandTypeId islandId = IslandTypeId.FromString(island);
				if (gameConfig.Islands.ContainsKey(islandId)) {
					board = new MergeBoardModel(
						gameConfig,
						islandId,
						MetaTime.Now,
						_ => { }
					);
					buttonTexts = BuildButtons();
				}
			}
			EditorGUILayout.EndHorizontal();

			int selectedCell = GUILayout.SelectionGrid(
				(board.Info.BoardHeight - selectedY - 1) * board.Info.BoardWidth + selectedX,
				buttonTexts,
				board.Info.BoardWidth
			);
			int newSelectedX = selectedCell % board.Info.BoardWidth;
			int newSelectedY = board.Info.BoardHeight - selectedCell / board.Info.BoardWidth - 1;

			if (newSelectedX != selectedX || newSelectedY != selectedY) {
				selectedX = newSelectedX;
				selectedY = newSelectedY;
				MergeTileModel tile = board[selectedX, selectedY];
				if (tile.HasItem) {
					type = tile.Item.Info.Type.ToString();
					level = tile.Item.Info.Level.ToString();
				} else {
					type = "N/A";
					level = "N/A";
				}
			}
			type = GUILayout.TextField(type);
			level = GUILayout.TextField(level);

			StringBuilder builder = new StringBuilder();
			for (int y = 0; y < board.Info.BoardHeight; y++) {
				for (int x = 0; x < board.Info.BoardWidth; x++) {
					MergeTileModel tile = board[x, y];
					if (tile.HasItem) {
						ItemModel item = tile.Item;
						builder.Append($"{island}	{x}	{y}	{item.Info.Type}	{item.Info.Level} FALSE\n");
					}
				}
			}
			GUILayout.TextField(builder.ToString());
		}

		private static string[] BuildButtons() {
			string[] buttons = new string[board.Info.BoardWidth * board.Info.BoardHeight];
			int i = 0;
			for (int y = 0; y < board.Info.BoardHeight; y++) {
				for (int x = 0; x < board.Info.BoardWidth; x++, i++) {
					int gameX = x;
					int gameY = board.Info.BoardHeight - y - 1;
					buttons[i] = BuildButtonText(board[gameX, gameY]);
				}
			}

			return buttons;
		}

		private static string BuildButtonText(MergeTileModel tile) {
			if (!tile.HasItem) {
				return "N/A";
			}

			return tile.Item.Info.Type + "/" + tile.Item.Info.Level;
		}

		static SharedGameConfig LoadGameConfig() {
			// Load config
			string              configPath      = Path.Combine(UnityEngine.Application.streamingAssetsPath, "SharedGameConfig.mpa");
			ConfigArchive       configArchive   = ConfigArchive.FromFile(configPath);
			SharedGameConfig    gameConfig      = (SharedGameConfig)GameConfigUtil.ImportSharedConfig(configArchive);
			return gameConfig;
		}
	}
}
