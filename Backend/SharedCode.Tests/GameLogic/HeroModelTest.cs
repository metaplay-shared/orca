using System;
using System.Reflection;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class HeroModelTest {
		private TestModel tm;
		private PlayerModel player;
		private SharedGameConfig config;
		private IslandTypeId island;
		private MergeBoardModel board;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			player = tm.PlayerModel;
			config = tm.GameConfig;
			island = IslandTypeId.MainIsland;
			board = player.Islands[island].MergeBoard;
			tm.PrimaryBoard = board;

			tm.BoardDeleteAllItems();
		}

		[Test]
		public void HeroTask() {
			HeroTypeId heroCaptain = HeroTypeId.FromString("HeroCaptain");
			HeroModel heroModel = new HeroModel(config.Heroes[heroCaptain], player.CurrentTime);
			heroModel.AssignToBuilding(ChainTypeId.FromString("CafeOrca"));

			ulong randomSeed = 6523;
			PropertyInfo propertyInfo = typeof(HeroModel).GetProperty("Random");
			propertyInfo.SetValue(heroModel, RandomPCG.CreateFromSeed(randomSeed));

			OrderedSet<ChainTypeId> unlockedResources =
				new OrderedSet<ChainTypeId> { ChainTypeId.FromString("Orange") };
			// Update with empty set of unlocked resources
			UpdateHeroModel(heroModel, new OrderedSet<ChainTypeId>());
			Assert.Null(heroModel.CurrentTask);

			UpdateHeroModel(heroModel, unlockedResources);
			Assert.NotNull(heroModel.CurrentTask);
			Assert.AreEqual(1, heroModel.CurrentTask.Info.Id);

			for (int expectedTaskId = 2; expectedTaskId <= 19; expectedTaskId++) {
				heroModel.FulfillTask(player.CurrentTime);
				heroModel.CurrentTask.Claim(player.CurrentTime);
				UpdateHeroModel(heroModel, unlockedResources);
				Assert.NotNull(heroModel.CurrentTask);
				Assert.AreEqual(expectedTaskId, heroModel.CurrentTask.Info.Id);
			}

			// The sequence manifested using the random seed (randomSeed) set above.
			int[] expectedTaskIds = new[] { 8, 5, 19, 13, 15, 2, 6, 18, 9, 2 };
			for (int i = 0; i < 10; i++) {
				heroModel.FulfillTask(player.CurrentTime);
				heroModel.CurrentTask.Claim(player.CurrentTime);
				UpdateHeroModel(heroModel, unlockedResources);
				Assert.NotNull(heroModel.CurrentTask);
				Assert.AreEqual(expectedTaskIds[i], heroModel.CurrentTask.Info.Id);
			}
		}

		private void UpdateHeroModel(HeroModel heroModel, OrderedSet<ChainTypeId> unlockedResources) {
			heroModel.Update(config, player.Level.Level, unlockedResources, player.CurrentTime, player.ClientListener);
		}
	}
}
