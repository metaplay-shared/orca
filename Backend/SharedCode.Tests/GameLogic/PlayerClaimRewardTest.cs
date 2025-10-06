using System.Collections.Generic;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	public class PlayerClaimRewardTest {
		private TestModel tm;
		private PlayerModel player;
		private MergeBoardModel board;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			board = player.Islands[IslandTypeId.MainIsland].MergeBoard;
		}

		[Test]
		public void NoRewardToClaim() {
			CollectionAssert.IsEmpty(player.Rewards);
			tm.AssertAction(new PlayerClaimReward(IslandTypeId.None), ActionResult.NotEnoughResources);
			Assert.Zero(tm.ClientListener.OnRewardClaimedCallCount);
			Assert.Zero(tm.AnalyticsEventRecorder.TotalCount);
		}

		[Test]
		public void ClaimTwoEmptyRewards() {
			RewardModel reward = CreateReward(gems: 0, gold: 0);
			player.Rewards.Add(reward);
			player.Rewards.Add(reward);

			// Claiming two empty rewards should not change the player model (e.g. gems, gold)
			// but should trigger sending analytics events and calling client listener.
			tm.AssertAction(new PlayerClaimReward(IslandTypeId.None));
			Assert.AreEqual(1, player.Rewards.Count);
			Assert.AreEqual(tm.InitialGold, player.Wallet.Gold.Value);
			Assert.AreEqual(tm.InitialGems, player.Wallet.Gems.Value);
			Assert.AreEqual(tm.InitialIslandTokens, player.Wallet.IslandTokens.Value);

			tm.AssertAction(new PlayerClaimReward(IslandTypeId.None));
			Assert.AreEqual(0, player.Rewards.Count);
			Assert.AreEqual(tm.InitialGold, player.Wallet.Gold.Value);
			Assert.AreEqual(tm.InitialGems, player.Wallet.Gems.Value);
			Assert.AreEqual(tm.InitialIslandTokens, player.Wallet.IslandTokens.Value);

			CollectionAssert.IsEmpty(board.ItemHolder);
			Assert.AreEqual(2, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerRewardClaimed)));
			Assert.AreEqual(2, tm.ClientListener.OnRewardClaimedCallCount);
		}

		[Test]
		public void ClaimSingleReward() {
			RewardModel reward = CreateReward(
				gems: 20,
				gold: 35,
				new ItemCountInfo(ChainTypeId.FromString("Orange"), level: 1, count: 3)
			);
			player.Rewards.Add(reward);

			tm.AssertAction(new PlayerClaimReward(IslandTypeId.None));
			CollectionAssert.IsEmpty(player.Rewards);
			Assert.AreEqual(tm.InitialGold + 35, player.Wallet.Gold.Value);
			Assert.AreEqual(tm.InitialGems + 20, player.Wallet.Gems.Value);

			// The reward chest will be placed on the board i.e. it won't be in the item holder queue.
			List<ItemModel> itemHolder = board.ItemHolder;
			Assert.AreEqual(0, itemHolder.Count);
			Assert.AreEqual(1, tm.AnalyticsEventRecorder.EventCount(typeof(PlayerRewardClaimed)));
			Assert.AreEqual(1, tm.ClientListener.OnRewardClaimedCallCount);
			Assert.AreEqual(1, tm.ClientListener.OnItemCreatedOnBoardCalls.Count);

			OnItemCreatedOnBoardArgs args = tm.ClientListener.OnItemCreatedOnBoardCalls[0];
			ItemModel rewardChest = board[args.ToX, args.ToY].Item;
			Assert.AreEqual("LevelUpRewards", rewardChest.Info.Type.Value);
			Assert.AreEqual(1, rewardChest.Info.Level);
			Assert.AreEqual(3, rewardChest.Creator.ItemCount);
			LevelId<ChainTypeId> orangeOne = new LevelId<ChainTypeId>(ChainTypeId.FromString("Orange"), 1);
			Assert.AreEqual(orangeOne, rewardChest.Creator.ItemQueue[0]);
			Assert.AreEqual(orangeOne, rewardChest.Creator.ItemQueue[1]);
			Assert.AreEqual(orangeOne, rewardChest.Creator.ItemQueue[2]);
		}

		private RewardModel CreateReward(int gems, int gold, params ItemCountInfo[] items) {
			string chestType = "LevelUpRewards";
			int chestLevel = 1;
			RewardType rewardType = RewardType.PlayerLevel;
			int rewardLevel = 1;

			List<ResourceInfo> resources = new List<ResourceInfo>();
			if (gems > 0) {
				resources.Add(new ResourceInfo(CurrencyTypeId.Gems, gems));
			}

			if (gold > 0) {
				resources.Add(new ResourceInfo(CurrencyTypeId.Gold, gold));
			}

			List<ItemCountInfo> itemCounts = new List<ItemCountInfo>(items);
			RewardMetadata metadata = new RewardMetadata() {
				Type = rewardType,
				Level = rewardLevel
			};

			return new RewardModel(resources, itemCounts, ChainTypeId.FromString(chestType), chestLevel, metadata);
		}
	}
}
