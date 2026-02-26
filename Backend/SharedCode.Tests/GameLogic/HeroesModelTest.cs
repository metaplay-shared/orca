using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class HeroesModelTest {
		private static SharedGameConfig config;
		private MetaTime time;
		private HeroesModel heroes;

		[OneTimeSetUp]
		public void StartTestFixture() {
			config = CommonUtils.LoadGameConfig();
		}

		[SetUp]
		public void StartTest() {
			time = MetaTime.Now;
			heroes = new HeroesModel(config);
		}

		[Test]
		public void UnlocksHero() {
			// The correct item unlocks a hero
			ItemModel item = new ItemModel(ChainTypeId.FromString("HeroCaptain"), 4, config, time, true);
			Assert.IsTrue(heroes.UnlocksHero(config, item));

			// Lower levels don't unlock a hero
			item = new ItemModel(ChainTypeId.FromString("HeroCaptain"), 3, config, time, true);
			Assert.IsFalse(heroes.UnlocksHero(config, item));

			// Wrong hero item type does not unlock a hero
			item = new ItemModel(ChainTypeId.FromString("HeroCook"), 7, config, time, true);
			Assert.IsFalse(heroes.UnlocksHero(config, item));
		}

		[Test]
		public void UnlockHero() {
			heroes.UnlockHero(config, _ => { }, EmptyPlayerModelClientListener.Instance, time);
			Assert.AreEqual(HeroTypeId.FromString("HeroCook"), heroes.CurrentHero);
			Assert.AreEqual(
				ChainTypeId.FromString("CafeOrca"),
				heroes.Heroes[HeroTypeId.FromString("HeroCaptain")].Building
			);

			heroes.UnlockHero(config, _ => { }, EmptyPlayerModelClientListener.Instance, time);
			Assert.AreEqual(HeroTypeId.FromString("HeroTourist"), heroes.CurrentHero);
			Assert.AreEqual(
				ChainTypeId.FromString("CafeOrca"),
				heroes.Heroes[HeroTypeId.FromString("HeroCook")].Building
			);

			heroes.UnlockHero(config, _ => { }, EmptyPlayerModelClientListener.Instance, time);
			Assert.AreEqual(HeroTypeId.FromString("HeroTest1"), heroes.CurrentHero);
			Assert.AreEqual(
				ChainTypeId.FromString("Dock"),
				heroes.Heroes[HeroTypeId.FromString("HeroTourist")].Building
			);

			heroes.UnlockHero(config, _ => { }, EmptyPlayerModelClientListener.Instance, time);
			Assert.AreEqual(HeroTypeId.None, heroes.CurrentHero);
			Assert.AreEqual(
				ChainTypeId.FromString("Workshop"),
				heroes.Heroes[HeroTypeId.FromString("HeroTest1")].Building
			);
		}
	}
}
