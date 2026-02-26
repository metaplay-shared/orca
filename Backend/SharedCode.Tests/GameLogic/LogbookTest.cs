using System;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class LogbookTest {
		private TestModel tm;
		private PlayerModel player;
		private SharedGameConfig config;
		private IslandTypeId island;
		private MergeBoardModel board;
		private MetaDictionary<LogbookChapterId, LogbookChapterModel> chapters;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel(MetaTime.FromDateTime(new DateTime(2005, 1, 1, 1, 55, 0)));
			player = tm.PlayerModel;
			player.PrivateProfile.FeaturesEnabled.Add(FeatureTypeId.DailyTaskEvents);
			config = tm.GameConfig;
			island = IslandTypeId.MainIsland;
			board = player.Islands[island].MergeBoard;
			tm.PrimaryBoard = board;
			chapters = player.Logbook.Chapters;

			tm.BoardDeleteAllItems();
		}

		private void OpenChapter(int chapter) {
			for (int i = 1; i < chapter; i++) {
				chapters[LogbookChapterId.FromString($"Chapter{i}")].ClaimChapterReward();
			}

			player.Logbook.Refresh(config, player.CurrentTime, player, player.ClientListener);
			tm.AssertAction(new PlayerOpenLogbookChapter(LogbookChapterId.FromString($"Chapter{chapter}")));
		}

		[Test]
		public void TaskTypeMerge() {
			OpenChapter(4);
			board.CreateItem(2, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(3, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(4, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(5, 0, tm.CreateItem("LogHouse:1"));

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 0, 3, 0));
			AssertTask(true, false, false, "Chapter4", "ch4-Merge");
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 4, 0, 5, 0));
			AssertTask(true, true, false, "Chapter4", "ch4-Merge");
		}

		[Test]
		public void TaskUseMine() {
			OpenChapter(5);
			board.CreateItem(3, 3, tm.CreateItem("LogMine:1"));

			tm.AssertAction(new PlayerUseMine(island, 3, 3));
			tm.TickProgress(MetaDuration.FromSeconds(3)); // Wait items to be mined
			AssertTask(true, true, false, "Chapter5", "ch5-UseMine");
			tm.AssertAction(new PlayerClaimMinedItems(island, 3, 3));
			tm.AssertAction(new PlayerRepairMine(island, 3, 3));
			AssertTask(true, true, false, "Chapter5", "ch5-RepairMine");
		}

		[Test]
		public void TaskUseBuilder() {
			OpenChapter(6);
			board.CreateItem(3, 3, tm.CreateItem("LogHouse:7"));

			tm.AssertAction(new PlayerUseBuilder(island, 3, 3));
			AssertTask(true, true, false, "Chapter6", "ch6-UseBuilder");
		}

		[Test]
		public void TaskUseBooster() {
			OpenChapter(7);
			board.CreateItem(3, 3, tm.CreateItem("LogHouse:7"));
			board.CreateItem(4, 3, tm.CreateItem("BronzeBuilder:3"));
			board.CreateItem(0, 1, tm.CreateItem("LogHouse:2"));
			board.CreateItem(1, 1, tm.CreateItem("BronzeWildcard:4"));

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 4, 3, 3, 3));
			AssertTask(true, false, false, "Chapter7", "ch7-UseBooster");
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 1, 1, 0, 1));
			AssertTask(true, true, false, "Chapter7", "ch7-UseBooster");
		}

		[Test]
		public void TaskUnlockIsland() {
			OpenChapter(8);
			AssertTask(true, false, false, "Chapter8", "ch8-UnlockIsland");

			player.Wallet.IslandTokens.Earned = 100;
			tm.AssertAction(new PlayerRevealIsland(IslandTypeId.FromString("StoneIsland")));
			tm.AssertAction(new PlayerUnlockIsland(IslandTypeId.FromString("StoneIsland")));
			// Task should not be completed when StoneIsland is unlocked (LogIsland should be unlocked instead)
			AssertTask(true, false, false, "Chapter8", "ch8-UnlockIsland");

			tm.AssertAction(new PlayerRevealIsland(IslandTypeId.FromString("LogIsland")));
			tm.AssertAction(new PlayerUnlockIsland(IslandTypeId.FromString("LogIsland")));
			AssertTask(true, true, false, "Chapter8", "ch8-UnlockIsland");
		}

		[Test]
		public void TaskIslandAlreadyUnlocked() {
			player.Wallet.IslandTokens.Earned = 100;
			tm.AssertAction(new PlayerRevealIsland(IslandTypeId.FromString("LogIsland")));
			tm.AssertAction(new PlayerUnlockIsland(IslandTypeId.FromString("LogIsland")));
			OpenChapter(8);
			// Task should be completed because the island was already unlocked when its chapter was opened.
			AssertTask(true, true, false, "Chapter8", "ch8-UnlockIsland");
		}

		[Test]
		public void TaskOpenChest() {
			OpenChapter(9);
			board.CreateItem(0, 1, tm.CreateItem("LogChest:3"));

			tm.AssertAction(new PlayerUseMine(island, 0, 1));
			AssertTask(true, true, false, "Chapter9", "ch9-OpenChest");
		}

		[Test]
		public void TaskCollectItem() {
			OpenChapter(10);
			board.CreateItem(0, 1, tm.CreateItem("Orange:1"));
			board.CreateItem(1, 1, tm.CreateItem("Orange:1"));
			board.CreateItem(0, 2, tm.CreateItem("Gold:3"));
			board.CreateItem(1, 2, tm.CreateItem("Gold:4"));
			board.CreateItem(2, 2, tm.CreateItem("Gold:5"));
			board.CreateItem(6, 2, tm.CreateItem("Gem:3"));

			player.Wallet.IslandTokens.Earned = 100;
			tm.AssertAction(new PlayerRevealIsland(IslandTypeId.FromString("LogIsland")));
			tm.AssertAction(new PlayerUnlockIsland(IslandTypeId.FromString("LogIsland")));
			IslandTypeId logIsland = IslandTypeId.FromString("LogIsland");
			MergeBoardModel logIslandBoard = player.Islands[logIsland].MergeBoard;

			// Collecting Gold:4 on LogIsland (island other than MainIsland) simply sends
			// the gold to MainIsland and thus the action should not increment ItemCount task.
			logIslandBoard.CreateItem(0, 0, tm.CreateItem("Gold:4"));
			logIslandBoard.CreateItem(1, 0, tm.CreateItem("Gold:4"));
			logIslandBoard.CreateItem(2, 0, tm.CreateItem("Gold:4"));
			tm.AssertAction(new PlayerCollectMergeItem(logIsland, 0, 0)); // Collect Gold:4 -> increment: 3
			tm.AssertAction(new PlayerCollectMergeItem(logIsland, 1, 0)); // Collect Gold:4 -> increment: 3
			tm.AssertAction(new PlayerCollectMergeItem(logIsland, 2, 0)); // Collect Gold:4 -> increment: 3
			AssertTask(true, false, false, "Chapter10", "ch10-CollectAnyGold");

			tm.AssertAction(new PlayerCollectMergeItem(island, 0, 1)); // Collect Orange:1
			AssertTask(true, false, false, "Chapter10", "ch10-CollectAnyItem");
			AssertTask(true, false, false, "Chapter10", "ch10-CollectOrange");
			AssertTask(true, false, false, "Chapter10", "ch10-CollectAnyGold");
			tm.AssertAction(new PlayerCollectMergeItem(island, 1, 1)); // Collect Orange:1
			AssertTask(true, false, false, "Chapter10", "ch10-CollectAnyItem");
			AssertTask(true, true, false, "Chapter10", "ch10-CollectOrange");
			AssertTask(true, false, false, "Chapter10", "ch10-CollectAnyGold");
			tm.AssertAction(new PlayerCollectMergeItem(island, 0, 2)); // Collect Gold:3 -> increment: 1
			tm.AssertAction(new PlayerCollectMergeItem(island, 1, 2)); // Collect Gold:4 -> increment: 3
			tm.AssertAction(new PlayerCollectMergeItem(island, 2, 2)); // Collect Gold:5 -> increment: 7
			AssertTask(true, false, false, "Chapter10", "ch10-CollectAnyItem");
			AssertTask(true, true, false, "Chapter10", "ch10-CollectOrange");
			AssertTask(true, true, false, "Chapter10", "ch10-CollectAnyGold");
			tm.AssertAction(new PlayerCollectMergeItem(island, 6, 2)); // Collect Gem:3
			AssertTask(true, true, false, "Chapter10", "ch10-CollectAnyItem");
		}

		[Test]
		public void BaseFunctionality() {
			board.CreateItem(2, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(3, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(4, 0, tm.CreateItem("LogHouse:1"));
			board.CreateItem(5, 0, tm.CreateItem("LogHouse:1"));

			board.CreateItem(0, 1, tm.CreateItem("StoneHouse:1"));
			board.CreateItem(1, 1, tm.CreateItem("StoneHouse:1"));
			board.CreateItem(2, 1, tm.CreateItem("StoneHouse:1"));
			board.CreateItem(3, 1, tm.CreateItem("StoneHouse:1"));
			board.CreateItem(2, 3, tm.CreateItem("StoneHouse:1"));
			board.CreateItem(3, 3, tm.CreateItem("StoneHouse:1"));

			// Only the first chapter should be open
			AssertChapter(ChapterState.Open, "Chapter1");
			AssertChapter(ChapterState.Locked, "Chapter2", "Chapter3", "Chapter4", "Chapter5");
			// Only the stone house task should be still be not-opened
			AssertTask(true, false, false, "Chapter1", "ch1-LogHouse2");
			AssertTask(true, false, false, "Chapter1", "ch1-LogHouse3");
			AssertTask(false, false, false, "Chapter1", "ch1-StoneHouse2");

			// Tasks not yet initialised for closed chapter
			LogbookChapterModel chapter2 = chapters[LogbookChapterId.FromString("Chapter2")];
			Assert.AreEqual(0, chapter2.Tasks.Count);

			Assert.AreEqual(0, tm.ClientListener.OnLogbookTaskModifiedCalls.Count);
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 0, 3, 0));
			Assert.AreEqual(1, tm.ClientListener.OnLogbookTaskModifiedCalls.Count);
			Assert.AreEqual(
				LogbookTaskId.FromString("ch1-LogHouse2"),
				tm.ClientListener.OnLogbookTaskModifiedCalls[0].Id
			);
			AssertTask(true, true, false, "Chapter1", "ch1-LogHouse2");
			AssertTask(true, false, false, "Chapter1", "ch1-LogHouse3");
			AssertTask(false, false, false, "Chapter1", "ch1-StoneHouse2");

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 4, 0, 5, 0));
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 3, 0, 5, 0));
			AssertTask(true, true, false, "Chapter1", "ch1-LogHouse2");
			AssertTask(true, true, false, "Chapter1", "ch1-LogHouse3");
			AssertTask(false, false, false, "Chapter1", "ch1-StoneHouse2");

			// Cannot claim reward for a nonexistent task
			tm.AssertAction(
				new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("nonexistent")),
				ActionResult.InvalidParam
			);
			// Cannot claim reward for a task not yet completed
			tm.AssertAction(
				new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch1-StoneHouse2")),
				ActionResult.InvalidState
			);
			// Cannot claim reward for a task in a locked chapter
			tm.AssertAction(
				new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch2-LogHouse2")),
				ActionResult.InvalidState
			);

			// Task not open yet -> input to item count task (StoneHouse2) is "wasted"
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 1, 1, 1));
			AssertTask(false, false, false, "Chapter1", "ch1-StoneHouse2");

			// Claim task rewards
			tm.ClientListener.OnLogbookTaskModifiedCalls.Clear();
			tm.AnalyticsEventRecorder.Events.Clear();
			tm.AssertAction(new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch1-LogHouse2")));
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerLogbookTaskRewardClaimed)));
			PlayerLogbookTaskRewardClaimed claimEvent =
				(PlayerLogbookTaskRewardClaimed)tm.AnalyticsEventRecorder.Events[0];
			Assert.AreEqual(LogbookTaskId.FromString("ch1-LogHouse2"), claimEvent.Task);

			tm.AssertAction(new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch1-LogHouse3")));
			Assert.AreEqual(3, tm.ClientListener.OnLogbookTaskModifiedCalls.Count);
			Assert.AreEqual(
				LogbookTaskId.FromString("ch1-LogHouse2"),
				tm.ClientListener.OnLogbookTaskModifiedCalls[0].Id
			);
			Assert.AreEqual(
				LogbookTaskId.FromString("ch1-LogHouse3"),
				tm.ClientListener.OnLogbookTaskModifiedCalls[1].Id
			);
			// Also the last task is modified (opened when dependencies get fulfilled)
			Assert.AreEqual(
				LogbookTaskId.FromString("ch1-StoneHouse2"),
				tm.ClientListener.OnLogbookTaskModifiedCalls[2].Id
			);

			AssertTask(true, true, true, "Chapter1", "ch1-LogHouse2");
			AssertTask(true, true, true, "Chapter1", "ch1-LogHouse3");
			AssertTask(true, false, false, "Chapter1", "ch1-StoneHouse2");

			// Complete last task in chapter 1
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 1, 3, 1));
			AssertTask(true, false, false, "Chapter1", "ch1-StoneHouse2");
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 3, 3, 3));
			AssertTask(true, true, false, "Chapter1", "ch1-StoneHouse2");

			// Chapter not complete yet (last task reward unclaimed) -> cannot claim chapter reward
			tm.AssertAction(
				new PlayerClaimLogbookChapterReward(LogbookChapterId.FromString("Chapter1")),
				ActionResult.InvalidState
			);
			tm.AssertAction(
				new PlayerClaimLogbookChapterReward(LogbookChapterId.FromString("nonexistent")),
				ActionResult.InvalidParam
			);

			AssertChapter(ChapterState.Locked, "Chapter2", "Chapter3", "Chapter4");
			Assert.AreEqual(0, tm.ClientListener.OnLogbookChapterModifiedCalls.Count);
			int gold = player.Wallet.Gold.Value;
			tm.AssertAction(new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch1-StoneHouse2")));
			Assert.AreEqual(gold + 4, player.Wallet.Gold.Value);

			Assert.AreEqual(1, tm.ClientListener.OnLogbookChapterModifiedCalls.Count);
			Assert.AreEqual(
				LogbookChapterId.FromString("Chapter1"),
				tm.ClientListener.OnLogbookChapterModifiedCalls[0].Id
			);

			AssertTask(true, true, true, "Chapter1", "ch1-StoneHouse2");
			AssertChapter(ChapterState.Complete, "Chapter1");
			AssertChapter(ChapterState.Locked, "Chapter2", "Chapter3", "Chapter4");

			// Claim chapter reward
			Assert.AreEqual(0, tm.ClientListener.OnLogbookChapterUnlockedCalls.Count);
			tm.AnalyticsEventRecorder.Events.Clear();
			tm.AssertAction(new PlayerClaimLogbookChapterReward(LogbookChapterId.FromString("Chapter1")));
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerLogbookChapterRewardClaimed)));
			PlayerLogbookChapterRewardClaimed chapterClaimEvent =
				(PlayerLogbookChapterRewardClaimed)tm.AnalyticsEventRecorder.Events[0];
			Assert.AreEqual(LogbookChapterId.FromString("Chapter1"), chapterClaimEvent.Chapter);

			Assert.AreEqual(1, player.Rewards.Count);
			RewardModel reward = player.Rewards[0];
			Assert.AreEqual(
				reward.ToString(),
				"RewardModel[resources: [5*Gems,10*Gold], items: [3*LogHouse:2,5*IslandToken:1]]"
			);

			Assert.AreEqual(2, tm.ClientListener.OnLogbookChapterModifiedCalls.Count);
			Assert.AreEqual(
				LogbookChapterId.FromString("Chapter1"),
				tm.ClientListener.OnLogbookChapterModifiedCalls[1].Id
			);
			Assert.AreEqual(1, tm.ClientListener.OnLogbookChapterUnlockedCalls.Count);
			Assert.AreEqual(
				LogbookChapterId.FromString("Chapter2"),
				tm.ClientListener.OnLogbookChapterUnlockedCalls[0].Id
			);
			AssertChapter(ChapterState.RewardClaimed, "Chapter1");
			AssertChapter(ChapterState.Opening, "Chapter2");
			AssertChapter(ChapterState.Locked, "Chapter3", "Chapter4");

			// Chapter 2
			board.CreateItem(0, 2, tm.CreateItem("LogHouse:1"));
			board.CreateItem(1, 2, tm.CreateItem("LogHouse:1"));
			board.CreateItem(2, 2, tm.CreateItem("LogHouse:1"));
			board.CreateItem(3, 2, tm.CreateItem("LogHouse:1"));
			board.CreateItem(4, 2, tm.CreateItem("LogHouse:1"));
			board.CreateItem(5, 2, tm.CreateItem("LogHouse:1"));
			board.CreateItem(3, 4, tm.CreateItem("CafeOrca:1"));

			AssertTask(true, false, false, "Chapter2", "ch2-LogHouse2");
			AssertTask(true, false, false, "Chapter2", "ch2-LogHouse3");
			AssertTask(false, false, false, "Chapter2", "ch2-HeroTasks");
			// Input to ch2-LogHouse2 wasted because chapter not opened yet
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 2, 1, 2));

			tm.AssertAction(new PlayerOpenLogbookChapter(LogbookChapterId.FromString("Chapter2")));
			AssertChapter(ChapterState.RewardClaimed, "Chapter1");
			AssertChapter(ChapterState.Open, "Chapter2");
			AssertChapter(ChapterState.Locked, "Chapter3", "Chapter4");

			tm.AssertAction(new PlayerMoveItemOnBoard(island, 2, 2, 3, 2));
			AssertTask(true, false, false, "Chapter2", "ch2-LogHouse2");
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 4, 2, 5, 2));
			AssertTask(true, true, false, "Chapter2", "ch2-LogHouse2");
			AssertTask(true, false, false, "Chapter2", "ch2-LogHouse3");
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 1, 2, 3, 2));
			AssertTask(true, true, false, "Chapter2", "ch2-LogHouse3");
			AssertTask(false, false, false, "Chapter2", "ch2-HeroTasks");

			tm.AssertAction(new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch2-LogHouse2")));
			tm.AssertAction(new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch2-LogHouse3")));

			// Unlock hero to be able to complete hero tasks
			board.CreateItem(2, 5, tm.CreateItem("HeroCaptain:4"));
			tm.AssertAction(new PlayerCollectMergeItem(island, 2, 5));
			player.Inventory.Resources[CurrencyTypeId.FromString("Orange")] = 100;
			player.Inventory.UnlockResourceItem(ChainTypeId.FromString("Orange"));

			player.Tick(NullChecksumEvaluator.Context); // Updates current hero task
			tm.AssertAction(new PlayerFulfillHeroTask(HeroTypeId.FromString("HeroCaptain")));
			tm.TickProgress(MetaDuration.FromMinutes(1));
			tm.AssertAction(new PlayerClaimHeroTaskRewards(HeroTypeId.FromString("HeroCaptain")));
			AssertTask(true, false, false, "Chapter2", "ch2-HeroTasks");

			player.Tick(NullChecksumEvaluator.Context); // Updates current hero task
			tm.AssertAction(new PlayerFulfillHeroTask(HeroTypeId.FromString("HeroCaptain")));
			tm.TickProgress(MetaDuration.FromMinutes(1));
			tm.AssertAction(new PlayerClaimHeroTaskRewards(HeroTypeId.FromString("HeroCaptain")));
			player.Tick(NullChecksumEvaluator.Context); // Updates current hero task
			AssertTask(true, true, false, "Chapter2", "ch2-HeroTasks");

			tm.AssertAction(new PlayerClaimLogbookTaskReward(LogbookTaskId.FromString("ch2-HeroTasks")));
			AssertTask(true, true, true, "Chapter2", "ch2-HeroTasks");
			tm.AssertAction(new PlayerClaimLogbookChapterReward(LogbookChapterId.FromString("Chapter2")));
			AssertChapter(ChapterState.RewardClaimed, "Chapter1", "Chapter2");
			AssertChapter(ChapterState.Opening, "Chapter3");
			AssertChapter(ChapterState.Locked, "Chapter4");

			// Chapter 3
			tm.AssertAction(new PlayerOpenLogbookChapter(LogbookChapterId.FromString("Chapter3")));
			AssertChapter(ChapterState.RewardClaimed, "Chapter1", "Chapter2");
			AssertChapter(ChapterState.Open, "Chapter3");
			AssertChapter(ChapterState.Locked, "Chapter4");

			board.CreateItem(0, 2, tm.CreateItem("IslandToken:1"));
			board.CreateItem(1, 2, tm.CreateItem("IslandToken:1"));
			tm.TickProgress(MetaDuration.FromMinutes(10), 100); // Progress past the start of daily tasks (LogbookTest)
			AssertTask(true, false, false, "Chapter3", "ch3-SingleDailyTasks");
			AssertTask(true, false, false, "Chapter3", "ch3-DailyTasks");
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 1, 1, 3, 1)); // Merge stone houses
			tm.AssertAction(new PlayerMoveItemOnBoard(island, 0, 2, 1, 2)); // Merge island tokens
			AssertTask(true, false, false, "Chapter3", "ch3-SingleDailyTasks");
			AssertTask(true, false, false, "Chapter3", "ch3-DailyTasks");
			tm.AssertAction(new PlayerClaimDailyTaskReward(EventId.FromString("LogbookTest"), 0));
			tm.AssertAction(new PlayerClaimDailyTaskReward(EventId.FromString("LogbookTest"), 1));
			AssertTask(true, true, false, "Chapter3", "ch3-SingleDailyTasks");
			AssertTask(true, true, false, "Chapter3", "ch3-DailyTasks");
		}

		private void AssertChapter(ChapterState expected, params string[] chapterIds) {
			foreach (string chapterId in chapterIds) {
				ChapterState actual = chapters[LogbookChapterId.FromString(chapterId)].State;
				Assert.AreEqual(expected, actual, $"Chapter {chapterId} expected to be {expected} (was {actual})");
			}
		}

		private void AssertTask(
			bool isOpen,
			bool isComplete,
			bool isClaimed,
			string chapterId,
			params string[] taskIds
		) {
			LogbookChapterModel chapter = chapters[LogbookChapterId.FromString(chapterId)];
			foreach (string taskId in taskIds) {
				LogbookTaskModel task = chapter.Tasks[LogbookTaskId.FromString(taskId)];
				Assert.AreEqual(isOpen, task.IsOpen, $"Task {taskId}: expected isOpen to be {isOpen}");
				Assert.AreEqual(isComplete, task.IsComplete, $"Task {taskId}: expected isComplete to be {isComplete}");
				Assert.AreEqual(isClaimed, task.IsClaimed, $"Task {taskId}: expected isClaimed to be {isClaimed}");
			}
		}
	}
}
