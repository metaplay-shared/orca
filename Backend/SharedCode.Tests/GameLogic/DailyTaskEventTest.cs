using System;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class DailyTaskEventTest {
		private TestModel tm;
		private PlayerModel player;
		private MockPlayerModelClientListener clientListener;
		private SharedGameConfig config;
		private MergeBoardModel board;
		private IslandTypeId island;

		private void InitBoard() {
			tm.BoardDeleteAllItems(board);
			board.CreateItem(3, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(4, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(5, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(6, 0, tm.CreateItem("LogHouse:1"));

			board.CreateItem(0, 1, tm.CreateItem("LogHouse:2"));
			board.CreateItem(1, 1, tm.CreateItem("LogHouse:2"));
			board.CreateItem(2, 1, tm.CreateItem("LogHouse:2"));
			board.CreateItem(3, 1, tm.CreateItem("LogHouse:2"));

			board.CreateItem(0, 2, tm.CreateItem("LogHouse:3"));
			board.CreateItem(1, 2, tm.CreateItem("LogHouse:3"));
			board.CreateItem(2, 2, tm.CreateItem("LogHouse:3"));
			board.CreateItem(3, 2, tm.CreateItem("LogHouse:3"));

			board.CreateItem(2, 5, tm.CreateItem("IslandToken:2"));
			board.CreateItem(3, 5, tm.CreateItem("IslandToken:2"));
			board.CreateItem(4, 5, tm.CreateItem("IslandToken:3"));
			board.CreateItem(4, 4, tm.CreateItem("IslandToken:3"));

			// Fill the item holder to simplify asserting the claimed rewards.
			board.CreateItem(0, 0, tm.CreateItem("Orange:1"));
			board.CreateItem(1, 0, tm.CreateItem("Orange:1"));
		}

		private void InitModels(DateTime time) {
			config = CommonUtils.LoadGameConfig();
			player = CommonUtils.CreatePlayerModel(MetaTime.FromDateTime(time), config);
			player.PrivateProfile.FeaturesEnabled.Add(FeatureTypeId.DailyTaskEvents);
			tm = new TestModel(player);
			clientListener = tm.ClientListener;
			board = player.Islands[IslandTypeId.MainIsland].MergeBoard;
			island = IslandTypeId.MainIsland;
			InitBoard();
		}

		[Test]
		public void DailyTasks() {
			InitModels(new DateTime(2021, 3, 5, 2, 0, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("DailyTest");
			DailyTaskEventModel model = player.DailyTaskEvents.SubEnsureHasState(
				config.DailyTaskEvents[eventId],
				player
			);
			// Event metadata
			Assert.AreEqual("Test daily tasks", model.Info.Description);
			Assert.AreEqual("Daily Test", model.Info.DisplayName);
			Assert.AreEqual("DailyTest, task set DailyTestTaskSet", model.Info.DisplayShortInfo);
			Assert.AreEqual(2, model.Tasks.Count);

			// Missing event
			EventId nonexistent = EventId.FromString("nonexistent");
			tm.AssertAction(new PlayerClaimDailyTaskReward(nonexistent, 0), ActionResult.NoSuchEvent);
			// Cannot claim rewards before completing tasks
			tm.AssertAction(new PlayerClaimDailyTaskReward(eventId, 0), ActionResult.InvalidState);
			tm.AssertAction(new PlayerClaimDailyTaskReward(eventId, 1), ActionResult.InvalidState);

			DailyTaskItem mergeTask = model.Tasks[0];
			DailyTaskItem mergeIslandTokenTask = model.Tasks[1];

			Assert.False(model.AdSeen);
			tm.AssertAction(new PlayerViewActivityEventAd(eventId));
			Assert.True(model.AdSeen);

			// Merge island tokens
			Assert.False(model.Completed());
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 0, 4, 0));
			Assert.AreEqual(1, clientListener.OnDailyTaskProgressMadeCalls.Count);
			OnDailyTaskProgressMadeArgs args = clientListener.OnDailyTaskProgressMadeCalls[0];
			Assert.AreEqual(EventId.FromString("DailyTest"), args.EventId);
			Assert.AreEqual(1, args.ProgressAmount);
			MergeBoardResourceContext context = (MergeBoardResourceContext)(args.Context);
			Assert.AreEqual(4, context.X);
			Assert.AreEqual(0, context.Y);
			Assert.False(model.Completed());
			Assert.AreEqual(1, mergeTask.CompletedAmount);
			Assert.AreEqual(0, mergeIslandTokenTask.CompletedAmount);
			Assert.AreEqual(0, model.UnclaimedRewards());
			Assert.AreEqual(0, model.ClaimedRewards());

			// Merge log houses item: nothing to be claimed yet
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 5, 3, 5));
			Assert.AreEqual(2, mergeTask.CompletedAmount);
			Assert.AreEqual(1, mergeIslandTokenTask.CompletedAmount);
			tm.AssertAction(new PlayerClaimDailyTaskReward(eventId, 0), ActionResult.InvalidState);
			tm.AssertAction(new PlayerClaimDailyTaskReward(eventId, 1), ActionResult.InvalidState);

			// Merge island tokens -> task "MergeIslandToken" (in the second slot) completed
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 5, 4, 5));
			Assert.AreEqual(3, mergeTask.CompletedAmount);
			Assert.AreEqual(2, mergeIslandTokenTask.CompletedAmount);
			Assert.AreEqual(1, model.UnclaimedRewards());
			Assert.AreEqual(0, model.ClaimedRewards());
			Assert.True(mergeIslandTokenTask.Completed);
			Assert.False(mergeIslandTokenTask.RewardClaimed);
			int goldBefore = player.Wallet.Gold.Value;
			tm.AssertAction(new PlayerClaimDailyTaskReward(eventId, 1));
			Assert.AreEqual(0, model.UnclaimedRewards());
			Assert.AreEqual(1, model.ClaimedRewards());
			Assert.AreEqual(goldBefore + 10, player.Wallet.Gold.Value);
			// Cannot claim the reward twice
			tm.AssertAction(new PlayerClaimDailyTaskReward(eventId, 1), ActionResult.InvalidState);
			Assert.AreEqual(goldBefore + 10, player.Wallet.Gold.Value);
			Assert.False(model.Completed());

			// Complete the other task i.e. "Merge"
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 1, 1, 1));
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 1, 3, 1));
			Assert.AreEqual(5, mergeTask.CompletedAmount);
			Assert.AreEqual(2, mergeIslandTokenTask.CompletedAmount);
			Assert.AreEqual(1, model.UnclaimedRewards());
			Assert.AreEqual(1, model.ClaimedRewards());
			Assert.True(model.Completed());
			int gemsBefore = player.Wallet.Gems.Value;

			// Progress 24h -> should trigger auto claim of task reward as well as the main reward for the day
			tm.TickProgress(new DateTime(2021, 3, 6, 2, 0, 1), 1000);
			Assert.AreEqual(gemsBefore + 6, player.Wallet.Gems.Value);
			Assert.AreEqual(1, player.Rewards.Count);

			// Tasks should have been reset (and level increased)
			mergeTask = model.Tasks[0];
			mergeIslandTokenTask = model.Tasks[1];
			Assert.AreEqual(0, mergeTask.CompletedAmount);
			Assert.AreEqual(0, mergeIslandTokenTask.CompletedAmount);
			Assert.AreEqual(1, model.Level);
			Assert.False(model.AdSeen);
		}

		[Test]
		public void LevelTransitions() {
			InitModels(new DateTime(2010, 1, 1, 2, 0, 0));
			player.Tick(NullChecksumEvaluator.Context);
			EventId eventId = EventId.FromString("LevelTest");
			DailyTaskEventModel model = player.DailyTaskEvents.SubEnsureHasState(
				config.DailyTaskEvents[eventId],
				player
			);
			int gemsBefore = player.Wallet.Gems.Value;
			Assert.AreEqual(5, model.Info.MaxLevel);
			Assert.AreEqual(6, model.Info.Rewards.Count);

			Assert.False(model.AdSeen);
			tm.AssertAction(new PlayerViewActivityEventAd(eventId));
			Assert.True(model.AdSeen);

			Assert.AreEqual(0, model.Level);
			clientListener.OnEventStateChangedCalls.Clear();
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(2, clientListener.OnEventStateChangedCalls.Count);
			// There should be two event state changed events: one for finalization, one for activation
			Assert.AreEqual(EventId.FromString("LevelTest"), clientListener.OnEventStateChangedCalls[0].EventId);
			Assert.AreEqual(EventId.FromString("LevelTest"), clientListener.OnEventStateChangedCalls[1].EventId);
			Assert.AreEqual(0, model.Level);

			// Progress in levels: 0 -> 5
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 0, 4, 0));
			tm.AssertAction(new PlayerClaimDailyTaskReward(eventId, 0));
			Assert.AreEqual(1, player.Rewards.Count);
			RewardModel reward0 = player.Rewards[0];
			Assert.AreEqual(ChainTypeId.FromString("Gem"), reward0.Items[0].Type);
			Assert.AreEqual(1, reward0.Items[0].Count);
			Assert.AreEqual(1, reward0.Items[0].Level);

			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(1, model.Level);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 5, 0, 6, 0));
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(2, model.Level);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 4, 0, 6, 0));
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(3, model.Level);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 1, 1, 1));
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(4, model.Level);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 1, 3, 1));
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(5, model.Level);

			// 3 inactive days: level drop from 5 to 3 (maximum of 2 level penalties i.e. LevelPenaltyRepeats)
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(4, model.Level);
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(3, model.Level);
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(3, model.Level); // Max level penalties reached

			// Complete daily tasks on 3 subsequent days: level 3 -> 5 and then back to 0
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 2, 1, 2));
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(4, model.Level);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 2, 3, 2));
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(5, model.Level);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 1, 2, 3, 2));
			tm.TickProgress(MetaDuration.FromMinutes(24 * 60 + 1), 1000);
			Assert.AreEqual(0, model.Level);

			// The daily main rewards should be placed in player rewards.
			Assert.AreEqual(8, player.Rewards.Count);
			RewardModel reward7 = player.Rewards[7];
			Assert.AreEqual(ChainTypeId.FromString("Gem"), reward7.Items[0].Type);
			Assert.AreEqual(1, reward7.Items[0].Count);
			Assert.AreEqual(6, reward7.Items[0].Level);
			RewardModel reward5 = player.Rewards[5];
			Assert.AreEqual(ChainTypeId.FromString("Gem"), reward5.Items[0].Type);
			Assert.AreEqual(1, reward5.Items[0].Count);
			Assert.AreEqual(4, reward5.Items[0].Level);

			// The gems from the daily task should have been accumulated to wallet.
			Assert.AreEqual(gemsBefore + 8, player.Wallet.Gems.Value);

			// Ad seen timestamp should not have been reset since ReshowAd == false.
			Assert.True(model.AdSeen);
		}
	}
}
