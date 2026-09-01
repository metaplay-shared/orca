using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Analytics;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic.Utils {
	/// <summary>
	/// <para>
	/// <c>TestModel</c> is the basic building block for most of the unit tests in the project. It provides an entry
	/// point to both the relevant model instances and the utility methods that help to write concise and readable test
	/// cases. It simplifies writing tests that involve <see cref="PlayerModel"/> and that possibly deal with
	/// elapsing time. Additionally, <c>TestModel</c> contains references to a set of mock objects (e.g.
	/// <see cref="IPlayerModelClientListener"/>) that record the calls made to them during the tests and hence allows
	/// asserting the correct behaviour of the models.
	/// </para>
	///
	/// <para>
	/// Implementation rationale: The utility methods are included in this class (as opposed to separate utility
	/// class(es)) to avoid the need for passing models (<see cref="PlayerModel"/>, <see cref="MergeBoardModel"/>, and
	/// <see cref="SharedGameConfig"/>) in every method call.
	/// </para>
	/// </summary>
	public class TestModel {
		public PlayerModel PlayerModel { get; private set; }
		/// <summary>
		/// <c>PrimaryBoard</c> holds the <see cref="MergeBoardModel"/> instance that a unit test primarily deals with.
		/// Vast majority of the unit tests interact only with a single island (and its merge board). In these cases
		/// the field (which is initially <c>null</c>) can be set by the user. This has the benefit of being able
		/// to call the various utility methods in this class without explicitly providing the <c>MergeBoardModel</c>.
		/// See for example <see cref="TestModel.AssertHasItem"/>.
		/// </summary>
		public MergeBoardModel PrimaryBoard { get; internal set; }
		public SharedGameConfig GameConfig { get; private set; }
		public MockPlayerModelClientListener ClientListener { get; private set; }
		public AnalyticsEventRecorder<IPlayerModelBase, PlayerEventBase> AnalyticsEventRecorder { get; private set; }

		public int InitialEnergy { get; private set; }
		public int InitialGold { get; private set; }
		public int InitialGems { get; private set; }
		public int InitialIslandTokens { get; private set; }
		public MetaTime StartTime { get; private set; }

		public TestModel(PlayerModel playerModel) {
			PlayerModel = playerModel;
			GameConfig = playerModel.GameConfig;

			InitialEnergy = playerModel.Merge.Energy.ProducedAtUpdate;
			InitialGold = playerModel.Wallet.Gold.Value;
			InitialGems = playerModel.Wallet.Gems.Value;
			InitialIslandTokens = playerModel.Wallet.IslandTokens.Value;
			StartTime = playerModel.CurrentTime;

			playerModel.OnInitialLogin(); // Simulate initial login to initialize resources
			playerModel.OnSessionStarted(); // Initialises e.g. islands
			playerModel.InitGame(); // Ensures events have their models initialized

			// Inject substitute objects only after the player model is initialised in order to ignore
			// any analytics events and calls to the client listener before the actual test cases.
			ClearAnalyticsEventRecorder();
			ClearClientListener();
		}

		public void ClearClientListener() {
			ClientListener = new MockPlayerModelClientListener();
			PlayerModel.ClientListener = ClientListener;
		}

		public void ClearAnalyticsEventRecorder() {
			AnalyticsEventRecorder = new AnalyticsEventRecorder<IPlayerModelBase, PlayerEventBase>();
			PlayerModel.AnalyticsEventHandler =
				new AnalyticsEventHandler<IPlayerModelBase, PlayerEventBase>(AnalyticsEventRecorder.RecordEvent);
		}

		private void EnsureNonNullBoard(MergeBoardModel board) {
			if (board == null) {
				throw new NullReferenceException(
					"MergeBoardModel is null. Provide merge board " +
					"a) by providing an instance explicitly as a method parameter " +
					"b) by setting the 'PrimaryBoard' field of TestModel in the test setup"
				);
			}
		}

		/*** ASSERT UTILITIES ***/

		/// <summary>
		/// <c>AssertAction</c> executes the given action and by default asserts that it succeeds.
		/// </summary>
		/// <param name="action">action to execute</param>
		/// <param name="expected">expected result (<see cref="MetaActionResult.Success"/> if omitted)</param>
		public void AssertAction(PlayerAction action, MetaActionResult expected = null) {
			expected ??= MetaActionResult.Success;
			MetaActionResult actualResult = action.Execute(PlayerModel, commit: true);
			Assert.AreEqual(
				expected,
				actualResult,
				$"Executing <{action.GetType().Name}> returned '{actualResult.Name}' but expected '{expected}'"
			);
		}

		// TODO: how to combine this with the overloaded method above?
		public void AssertAction(PlayerActionCore<IPlayerModelBase> action, MetaActionResult expected = null) {
			expected ??= MetaActionResult.Success;
			MetaActionResult actualResult = action.Execute(PlayerModel, commit: true);
			Assert.AreEqual(
				expected,
				actualResult,
				$"Executing <{action.GetType().Name}> returned '{actualResult.Name}' but expected '{expected}'"
			);
		}

		/// <summary>
		/// <c>AssertItem</c> is a convenience method for checking that an <see cref="ItemModel"/> is of expected
		/// type and level.
		/// </summary>
		/// <param name="expected">expected item type and level expressed as string e.g. "LogHouse:3"</param>
		/// <param name="actual">actual item model</param>
		public void AssertItem(string expected, ItemModel actual) {
			ItemModel expectedModel = CreateItem(expected);
			Assert.AreEqual(expectedModel.Info.Type, actual.Info.Type);
			Assert.AreEqual(expectedModel.Info.Level, actual.Info.Level);
		}

		/// <summary>
		/// <c>AssertHasItem</c> asserts that there's a specified item (e.g. "Orange:1") at the given coordinates
		/// on the board.
		/// </summary>
		public void AssertHasItem(
			int x,
			int y,
			string item,
			ItemState state = ItemState.Free,
			MergeBoardModel board = null
		) {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			ItemModel actualItem = board[x, y].Item;
			LevelId<ChainTypeId> expected = CreateItemType(item);
			Assert.NotNull(actualItem, $"Expected '{expected}' in ({x},{y}) but found no item");
			LevelId<ChainTypeId> actual = new LevelId<ChainTypeId>(actualItem.Info.Type, actualItem.Info.Level);
			Assert.AreEqual(expected, actual, $"({x},{y}): expected {expected} but contains {actual}");
			ItemState actualState = actualItem.State;
			Assert.AreEqual(state, actualState, $"Item state at ({x},{y}) should be {state} but was {actualState}");
		}

		/// <summary>
		/// <c>AssertIsEmptyTile</c> asserts that there is an empty <see cref="TileType.Ground"/> or
		/// <see cref="TileType.ItemHolder"/> tile at the given coordinates.
		/// </summary>
		public void AssertIsEmptyTile(int x, int y, MergeBoardModel board = null) {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			MergeTileModel tile = board[x, y];
			Assert.NotNull(tile, $"Expected be an empty tile ({x},{y}) but was out of bounds");
			Assert.True(
				tile.Type == TileType.Ground || tile.Type == TileType.ItemHolder,
				$"Expected a Ground or ItemHolder tile ({x},{y}) but was {tile.Type}"
			);
			Assert.Null(tile.Item, $"Expected an empty tile ({x},{y}) but has item {tile:Item}");
		}

		/*** ITEM UTILITIES ***/

		public ItemModel CreateItem(string item, ItemState state = ItemState.Free, bool skipFreeForMerge = false) {
			LevelId<ChainTypeId> id = CreateItemType(item);
			ItemModel itemModel = new ItemModel(id.Type, id.Level, GameConfig, MetaTime.Epoch, false) {
				State = state,
				SkipFreeForMerge = skipFreeForMerge
			};
			return itemModel;
		}

		public LevelId<ChainTypeId> CreateItemType(string item) {
			string[] typeAndLevel = item.Split(":");
			Assert.That(typeAndLevel.Length, Is.EqualTo(2), "Invalid item string: {0}'", item);
			string type = typeAndLevel[0];
			int level = Int32.Parse(typeAndLevel[1]);
			return new LevelId<ChainTypeId>(ChainTypeId.FromString(type), level);
		}

		/*** DEBUG UTILITIES ***/

		public void PrintActivityEventState(string activityEventId, PlayerModel player = null) {
			player ??= PlayerModel;

			EventId eventId = EventId.FromString(activityEventId);
			Console.Out.WriteLine($"--- {player.CurrentTime} {activityEventId}");
			ActivityEventModel model = player.ActivityEvents.TryGetState(eventId);

			string state = player.ActivityEvents.IsInPreview(player.GameConfig.ActivityEvents[eventId], player)
				? "preview"
				: "<unknown>";
			if (model == null) {
				Console.Out.WriteLine($"  state: {state}");
			} else {
				if (player.ActivityEvents.IsInPreview(player.GameConfig.ActivityEvents[eventId], player)) {
					state = "preview";
				} else if (model.IsActive(player)) {
					state = "active";
				} else if (model.IsInReview(player.CurrentTime)) {
					state = "review";
				} else if (model.IsInCooldown(player.CurrentTime)) {
					state = "cooldown";
				}

				Console.Out.WriteLine($"  state: {state}, level: {model.EventLevel}");
			}
		}

		public void PrintEventStatuses(bool printInactive = false, bool printRange = false, PlayerModel player = null) {
			player ??= PlayerModel;

			Console.Out.WriteLine($"--- {player.CurrentTime}");
			List<IEventModel> eventModels = player.VisibleEventModels();
			foreach (IEventModel eventModel in eventModels) {
				int timeWidth = 18;
				MetaActivableVisibleStatus status = eventModel.Status(player);
				if (status == null) {
					if (printInactive) {
						Console.Out.WriteLine($"  ** {eventModel.EventId} - INACTIVE");
					}
				} else if (status is MetaActivableVisibleStatus.Active active) {
					Console.Out.WriteLine($"  ** {eventModel.EventId} - ACTIVE");
					if (printRange) {
						Console.Out.WriteLine($"{"range".PadLeft(timeWidth)}: {active.ScheduleEnabledRange}");
					}

					Console.Out.WriteLine($"{"start".PadLeft(timeWidth)}: {active.ActivationStartedAt}");
					Console.Out.WriteLine($"{"ending soon".PadLeft(timeWidth)}: {active.EndingSoonStartsAt}");
					Console.Out.WriteLine($"{"end".PadLeft(timeWidth)}: {active.ActivationEndsAt}");
				} else if (status is MetaActivableVisibleStatus.InPreview inPreview) {
					Console.Out.WriteLine($"  ** {eventModel.EventId} - PREVIEW");
					if (printRange) {
						Console.Out.WriteLine($"{"range".PadLeft(timeWidth)}: {inPreview.ScheduleEnabledRange}");
					}

					Console.Out.WriteLine($"{"start".PadLeft(timeWidth)}: {inPreview.ScheduleEnabledRange.Start}");
					Console.Out.WriteLine($"{"end".PadLeft(timeWidth)}: {inPreview.ScheduleEnabledRange.End}");
				} else if (status is MetaActivableVisibleStatus.EndingSoon endingSoon) {
					Console.Out.WriteLine($"  ** {eventModel.EventId} - ENDING SOON (ACTIVE)");
					if (printRange) {
						Console.Out.WriteLine($"{"range".PadLeft(timeWidth)}: {endingSoon.ScheduleEnabledRange}");
					}

					Console.Out.WriteLine($"{"start".PadLeft(timeWidth)}: {endingSoon.ActivationStartedAt}");
					Console.Out.WriteLine($"{"ending soon".PadLeft(timeWidth)}: {endingSoon.EndingSoonStartedAt}");
					Console.Out.WriteLine($"{"end".PadLeft(timeWidth)}: {endingSoon.ActivationEndsAt}");
				} else if (status is MetaActivableVisibleStatus.InReview inReview) {
					Console.Out.WriteLine($"  **	{eventModel.EventId} - REVIEW");
					if (printRange) {
						Console.Out.WriteLine($"     range: {inReview.ScheduleEnabledRange}");
					}

					Console.Out.WriteLine($"{"start".PadLeft(timeWidth)}: {inReview.ActivationStartedAt}");
					Console.Out.WriteLine($"{"ended".PadLeft(timeWidth)}: {inReview.ActivationEndedAt}");
					Console.Out.WriteLine($"{"inactive".PadLeft(timeWidth)}: {inReview.VisibilityEndsAt}");
				}
			}

			Console.Out.WriteLine("");
		}

		public void PrintPlayerRewards(string title = "Rewards:", PlayerModel player = null) {
			player ??= PlayerModel;

			Console.Out.WriteLine(title);
			for (int i = 0; i < player.Rewards.Count; i++) {
				RewardModel rewardModel = player.Rewards[i];
				Console.Out.WriteLine($"  {i}: {rewardModel}");
			}
		}

		public void PrintList<T>(List<T> list, string title = "List") {
			Console.Out.WriteLine($"{title} ({list.Count} items)");
			for (int i = 0; i < list.Count; i++) {
				Console.Out.WriteLine(" {0,3}: {1}", i, list[i]);
			}
		}

		public void PrintDict<T, V>(MetaDictionary<T, V> dict, string title = "Dictionary") {
			Console.Out.WriteLine($"{title} ({dict.Count} items)");
			foreach (MetaDictionary<T, V>.KeyValue keyValue in dict) {
				Console.Out.WriteLine($"  {keyValue.Key}: {keyValue.Value}");
			}
		}

		// Constants for printing merge boards
		private static readonly string[] TileSea = { "~~~", "~~~", "~~~" };
		private static readonly char[] TileItemHolder = { 'H', 'H', 'H' };
		private static readonly char[] TileShip = { 'S', 'S', 'S' };
		private static readonly string[] TileBigItem = { "@@@", "@@@", "@@@" };
		private static readonly int TileSize = 3;

		/// <summary>
		/// <c>PrintBoard</c> prints the text representation of a merge board. The textual representation is backed by
		/// a character array that can be thought of as a two dimensional grid with x and y coordinates growing
		/// to the right and up respectively (the same as the coordinates of a merge board). A tile is represented
		/// as 3x3 square. For example, a LogHouse of level 1 in state "Hidden" placed on a lock area with index 2 is
		/// drawn as
		/// <code>
		/// |---|
		/// |H  |
		/// |LH1|
		/// |  2|
		/// |---|
		/// </code>
		///
		/// An example of the whole board:
		/// below:
		/// <code>
		///    |~~~|~~~|   |   |   |~~~|~~~
		///  8 |~~~|~~~|   |   |   |~~~|~~~
		///    |~~~|~~~|  2|  2|  2|~~~|~~~
		/// ---|---|---|---|---|---|---|---
		///    |~~~|   |   |   |   |   |~~~
		///  7 |~~~|   |   |   |   |   |~~~
		///    |~~~|  2|  2|  2|  2|  2|~~~
		/// ---|---|---|---|---|---|---|---
		///    |~~~|   |   |   |   |   |
		///  6 |~~~|   |   |   |   |   |
		///    |~~~|  2|  2|  2|  2|  1|  1
		/// ---|---|---|---|---|---|---|---
		///    |~~~|H  |@@@|@@@|M  |H  |H
		///  5 |~~~|LH1|@@@|@@@|LH1|LH1|LH1
		///    |~~~|  2|@@@|@@@|   |  1|  1
		/// ---|---|---|---|---|---|---|---
		///    |~~~|~~~|   |@@@|M  |M  |P
		///  4 |~~~|~~~|SC1|@@@|LH1|LH1|LH1
		///    |~~~|~~~|   |@@@|   |   |
		/// ---|---|---|---|---|---|---|---
		///    |~~~|~~~|   |   |   |   |M
		///  3 |~~~|~~~|   |   |   |   |LH1
		///    |~~~|~~~|   |   |   |   |
		/// ---|---|---|---|---|---|---|---~
		///    |   |   |   |   |   |   |M
		///  2 |   |   |   |   |   |   |LH1
		///    |   |   |   |   |   |   |
		/// ---|---|---|---|---|---|---|---
		///    |   |   |   |   |~~~|~~~|~~~
		///  1 |   |   |   |   |~~~|~~~|~~~
		///    |   |   |   |   |~~~|~~~|~~~
		/// ---|---|---|---|---|---|---|---
		///    |HHH|HHH|   |   |   |   |
		///  0 |HHH|HHH|   |   |   |   |
		///    |HHH|HHH|   |   |   |   |
		/// ---|---|---|---|---|---|---|---
		///    |   |   |   |   |   |   |
		///    | 0 | 1 | 2 | 3 | 4 | 5 | 6
		///    |   |   |   |   |   |   |
		/// </code>
		///
		/// Above we have (board) coordinates shown on the sides, item holder on the lower left corner, sea tiles
		/// represented with tildes '~', and a 2x2 item (StoneCreator:1) at (2,4), and a lock area with index 2
		/// at the top.
		/// </summary>
		/// <param name="board">merge board to print</param>
		/// <param name="title">optional title to print before the merge board</param>
		public void PrintBoard(MergeBoardModel board = null, string title = null) {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			Console.Out.WriteLine(title ?? board.Info.Type.Value);
			int charBoardWidth = board.Info.BoardWidth + (board.Info.BoardWidth + 1) * TileSize;
			int charBoardHeight = board.Info.BoardHeight + (board.Info.BoardHeight + 1) * TileSize;
			char[] charBoard = new char[charBoardWidth * charBoardHeight];

			// Init chars to ' ' and draw lines between tiles
			for (int y = 0; y < charBoardHeight; y++) {
				for (int x = 0; x < charBoardWidth; x++) {
					char ch = ' ';
					if ((y + 1) % (TileSize + 1) == 0) {
						ch = '-';
					}

					if ((x + 1) % (TileSize + 1) == 0) {
						ch = '|';
					}

					charBoard[y * charBoardWidth + x] = ch;
				}
			}

			// Draw Y coordinates
			for (int boardY = 0; boardY < board.Info.BoardHeight; boardY++) {
				InsertTile(
					charBoard,
					charBoardWidth,
					0,
					(boardY + 1) * (TileSize + 1),
					new[] { "   ", $" {boardY} ", "   " }
				);
			}

			// Draw X coordinates
			for (int boardX = 0; boardX < board.Info.BoardWidth; boardX++) {
				InsertTile(
					charBoard,
					charBoardWidth,
					(boardX + 1) * (TileSize + 1),
					0,
					new[] { "   ", $" {boardX} ", "   " }
				);
			}

			// Draw tiles
			for (int boardY = 0; boardY < board.Info.BoardHeight; boardY++) {
				for (int boardX = 0; boardX < board.Info.BoardWidth; boardX++) {
					string[] tileTexts = TileToText(board, boardX, boardY);
					InsertTile(
						charBoard,
						charBoardWidth,
						(boardX + 1) * TileSize + boardX + 1,
						(boardY + 1) * TileSize + boardY + 1,
						tileTexts
					);
				}
			}

			// Actually print the board
			for (int y = charBoardHeight - 1; y >= 0; y--) {
				string line = "";
				for (int x = 0; x < charBoardWidth; x++) {
					line += charBoard[y * charBoardWidth + x].ToString();
				}

				Console.Out.WriteLine(line);
			}

			Console.Out.WriteLine("");
		}

		// Inserts a 3x3 area to the character grid.
		private void InsertTile(char[] charBoard, int charBoardWidth, int charX, int charY, string[] tileTexts) {
			for (int j = 0; j < Math.Min(TileSize, tileTexts.Length); j++) {
				string text = tileTexts[j];
				for (int i = 0; i < Math.Min(TileSize, text.Length); i++) {
					charBoard[(charY + j) * charBoardWidth + charX + i] = text[i];
				}
			}
		}

		// Returns a 3x3 character representation for a tile. The string at the first index of the result array
		// is the lowest row of the tile.
		private string[] TileToText(MergeBoardModel board, int x, int y) {
			MergeTileModel tile = board[x, y];
			if (tile.Type == TileType.Sea) {
				return TileSea;
			}

			char[] line2 = { ' ', ' ', ' ' };
			char[] line1 = { ' ', ' ', ' ' };
			char[] line0 = { ' ', ' ', ' ' };

			if (tile.Type == TileType.ItemHolder) {
				line2 = TileItemHolder;
				line1 = TileItemHolder;
				line0 = TileItemHolder;
			}

			if (tile.Type == TileType.Ship) {
				line2 = TileShip;
				line1 = TileShip;
				line0 = TileShip;
			}

			// Add lock area symbol (bottom right corner) if necessary
			if (board.LockArea[x, y] != LockAreaModel.NO_AREA) {
				line0[2] = board.LockArea[x, y];
			}

			ItemModel item = tile.Item;
			if (item != null) {
				if (!(item.X == x && item.Y == y)) {
					// Part of an item that is bigger than 1x1
					return TileBigItem;
				}

				string itemNameAbbreviation = ItemAbbreviation(item);
				line1 = itemNameAbbreviation.ToCharArray(0, 3);

				switch (item.State) {
					case ItemState.Hidden:
						line2[0] = 'H';
						break;
					case ItemState.PartiallyVisible:
						line2[0] = 'P';
						break;
					case ItemState.FreeForMerge:
						line2[0] = 'M';
						break;
				}

				if (item.Bubble) {
					line2[1] = 'B';
				}

				// Add BuildState symbol (bottom left corner)
				if (item.BuildState == ItemBuildState.Building) {
					line0[0] = 'B';
				} else if (item.BuildState == ItemBuildState.NotStarted) {
					line0[0] = 'N';
				}
			}

			return new[] { new string(line0), new string(line1), new string(line2) };
		}

		// Abbreviate the item name:
		//  * Name has multiple capital letters: LogHouse level 1 -> "LH1"
		//  * Only the first letter is capitalised: Gem level 3 -> "Ge3"
		private static string ItemAbbreviation(ItemModel item) {
			string itemName = item.Info.Type.Value;
			int itemLevel = item.Info.Level;
			string caps = Regex.Replace(item.Info.Type.Value, @"[a-z]+", "");
			return (caps.Length < 2 ? itemName.Substring(0, 2) : caps.Substring(0, 2)) + itemLevel;
		}

		public void PrintItemHolder(MergeBoardModel board = null, string title = null) {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			Console.Out.WriteLine($"Item holder: {title ?? board.Info.Type.Value} ({board.ItemHolder.Count} items)");
			for (int i = 0; i < board.ItemHolder.Count; i++) {
				Console.Out.WriteLine($"  {i} {board.ItemHolder[i]}");
			}
		}

		// PrintItems prints the items on the board.
		public void PrintItems(MergeBoardModel board = null, string title = "") {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			Console.Out.WriteLine($"# {board.TotalItemCount} item(s) on board: {title}");
			for (int y = 0; y < board.Info.BoardHeight; y++) {
				for (int x = 0; x < board.Info.BoardWidth; x++) {
					ItemModel item = board[x, y].Item;
					if (item != null) {
						Console.Out.WriteLine(
							$"  ({x},{y}): {item.Info.Type}:{item.Info.Level}:{item.State}:{item.BuildState}"
						);
					}
				}
			}
		}

		// PrintNonCompleteItems prints the items on the board whose build state is not complete.
		public void PrintNonCompleteItems(MergeBoardModel board = null, PlayerModel player = null, string title = "") {
			player ??= PlayerModel;
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			Console.Out.WriteLine($"# {board.TotalItemCount} item(s) on board: {title}");
			for (int y = 0; y < board.Info.BoardHeight; y++) {
				for (int x = 0; x < board.Info.BoardWidth; x++) {
					ItemModel item = board[x, y].Item;
					if (item != null && item.BuildState != ItemBuildState.Complete) {
						MetaDuration buildTimeLeft = item.Info.BuildTime;
						if (item.BuilderId > 0) {
							MetaTime completeAt = player.Builders.GetCompleteAt(item.BuilderId);
							buildTimeLeft = completeAt - player.CurrentTime;
						}

						Console.Out.WriteLine(
							$"  ({x},{y}): {item.Info.Type}:{item.Info.Level}:{item.BuildState}:{buildTimeLeft}"
						);
					}
				}
			}
		}

		/*** BOARD UTILITIES ***/

		// DeleteAllItems deletes all items on the given board.
		public void BoardDeleteAllItems(MergeBoardModel board = null) {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			for (int y = 0; y < board.Info.BoardHeight; y++) {
				for (int x = 0; x < board.Info.BoardWidth; x++) {
					ItemModel item = board[x, y].Item;
					if (item != null) {
						// Must use item.X, itemY (instead of x, y) to handle larger than 1x1 items correctly
						board.RemoveItem(item.X, item.Y, EmptyPlayerModelClientListener.Instance);
					}
				}
			}
		}

		// Place an item onto all empty tiles on the board
		public void BoardFill(ItemModel item, MergeBoardModel board = null, bool fillLockedAreas = true) {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			for (int y = 0; y < board.Info.BoardHeight; y++) {
				for (int x = 0; x < board.Info.BoardWidth; x++) {
					if (board[x, y].Item == null &&
						board[x, y].IsFree &&
						(board.LockArea.IsFree(x, y) || fillLockedAreas)) {
						ItemModel itemCopy = new ItemModel(
							item.Info.Type,
							item.Info.Level,
							GameConfig,
							MetaTime.Epoch,
							true
						);
						board.CreateItem(x, y, itemCopy);
					}
				}
			}
		}

		public void BoardApplyToAllItems(Action<ItemModel> action, MergeBoardModel board = null) {
			board ??= PrimaryBoard;
			EnsureNonNullBoard(board);

			foreach (var item in board.Items) {
				action(item);
			}
		}

		/*** TIME UTILITIES ***/

		/// <summary>
		/// <see cref="TickProgress"/> ticks the <see cref="PlayerModel"/> to progress in time.
		/// </summary>
		/// <param name="duration">time to progress</param>
		/// <param name="increment">how many ticks to progress before actually calling <c>PlayerModel.Tick()</c>.
		/// Specifying a value greater than 1 makes the method execute faster because <c>PlayerModel</c> is updated
		/// only after <paramref name="increment"/> ticks.</param>
		///	<param name="player">player model to tick</param>
		public void TickProgress(MetaDuration duration, int increment = 1, PlayerModel player = null) {
			player ??= PlayerModel;

			int ticks = duration.ToLocalTicks(PlayerModel.TicksPerSecond);
			for (int i = 0; i < ticks; i++) {
				if (increment > 1) {
					int cappedIncrement = Math.Min(increment, ticks - i);
					player.CurrentTick += cappedIncrement - 1;
					i += cappedIncrement - 1;
				}

				player.Tick(NullChecksumEvaluator.Context);
			}
		}

		public void TickProgress(DateTime time, int increment = 1, PlayerModel player = null) {
			player ??= PlayerModel;
			TickProgress(MetaTime.FromDateTime(time) - player.CurrentTime, increment, player);
		}

		/*** MISC UTILITIES ***/

		/// <summary>
		/// CreateIslandTask creates an <c>IslandTaskInfo</c> object.
		/// </summary>
		/// <param name="itemCounts">string representing <c>ItemCountInfo</c> object. For example, "5*LogHouse:2"
		/// corresponds to 5 LogHouse items of level 2</param>
		/// <returns></returns>
		public IslandTaskInfo CreateIslandTask(params string[] itemCounts) {
			IslandTaskInfo task = new IslandTaskInfo();
			List<ItemCountInfo> itemCountsList = new List<ItemCountInfo>();
			task.GetType().GetProperty("Items").SetValue(task, itemCountsList);

			foreach (var itemCountString in itemCounts) {
				string[] countAndItemString = itemCountString.Split("*");
				Assert.That(
					countAndItemString.Length,
					Is.EqualTo(2),
					"Invalid item count info string: {0}'",
					itemCountString
				);
				int count = Int32.Parse(countAndItemString[0]);

				string[] typeAndLevel = countAndItemString[1].Split(":");
				Assert.That(
					typeAndLevel.Length,
					Is.EqualTo(2),
					"Invalid item count info string: {0}'",
					itemCountString
				);
				string type = typeAndLevel[0];
				int level = Int32.Parse(typeAndLevel[1]);

				ItemCountInfo itemCountInfo = new ItemCountInfo(ChainTypeId.FromString(type), level, count);
				itemCountsList.Add(itemCountInfo);
			}

			return task;
		}
	}
}
