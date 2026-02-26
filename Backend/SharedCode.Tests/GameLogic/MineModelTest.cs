using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	public class MineModelTest {
		private TestModel tm;
		private SharedGameConfig config;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			config = tm.GameConfig;
		}

		[Test]
		public void MiningAndRepairing() {
			MineModel mine = new MineModel(config, MineTypeId.FromString("TestMine"));

			mine.StartMining(1);
			Assert.AreEqual(MineState.Mining, mine.State);
			Assert.AreEqual(1, mine.BuilderId);

			mine.StartRepairing(2);
			Assert.AreEqual(MineState.Repairing, mine.State);
			Assert.AreEqual(2, mine.BuilderId);
		}

		[Test]
		public void UpdateState() {
			MineModel mine = new MineModel(config, MineTypeId.FromString("TestMine"));

			mine.StartMining(1);
			mine.UpdateState(config); // Mining -> ItemsComplete
			Assert.AreEqual(MineState.ItemsComplete, mine.State);
			Assert.AreEqual(0, mine.BuilderId);

			mine.UpdateState(config); // ItemsComplete -> Idle
			Assert.AreEqual(MineState.Idle, mine.State);
			Assert.AreEqual(1, mine.MineCycle);

			mine.StartMining(1);
			mine.UpdateState(config); // Mining -> ItemsComplete
			mine.UpdateState(config); // ItemsComplete -> NeedsRepair
			Assert.AreEqual(MineState.NeedsRepair, mine.State);
			Assert.AreEqual(2, mine.MineCycle);

			mine.StartRepairing(1);
			mine.UpdateState(config); // Repairing -> Idle
			Assert.AreEqual(MineState.Idle, mine.State);
			Assert.AreEqual(0, mine.MineCycle);
			Assert.AreEqual(1, mine.RepairCycle);

			// Run one more cycle to trigger a level upgrade on the mine
			mine.StartMining(1);
			mine.UpdateState(config); // Mining -> ItemsComplete
			mine.UpdateState(config); // ItemsComplete -> Idle
			mine.StartMining(1);
			mine.UpdateState(config); // Mining -> ItemsComplete
			mine.UpdateState(config); // ItemsComplete -> NeedsRepair
			mine.StartRepairing(1);
			mine.UpdateState(config); // Repairing -> Idle

			Assert.AreEqual(2, mine.Info.Level);
		}
	}
}
