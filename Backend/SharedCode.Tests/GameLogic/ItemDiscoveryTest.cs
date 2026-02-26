using System.Collections.Generic;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class ItemDiscoveryTest {
		private TestModel tm;
		private SharedGameConfig config;

		[SetUp]
		public void StartTest() {
			tm = CommonUtils.CreateTestModel();
			config = tm.GameConfig;
		}

		[Test]
		public void EmptyItemDiscovery() {
			ItemDiscovery itemDiscovery = new ItemDiscovery();
			// In an empty ItemDiscovery everything should be NotDiscovered and there should be nothing to claim.
			AssertNotDiscovered(itemDiscovery, "LogHouse:1");
			Assert.False(itemDiscovery.SomethingToClaim(config));
			Assert.False(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Resources")));
			Assert.IsEmpty(itemDiscovery.SomethingToClaimCategories(config));
		}

		[Test]
		public void SingleDiscovery() {
			ItemDiscovery itemDiscovery = new ItemDiscovery();
			// Discover LogHouse:1
			Assert.True(itemDiscovery.SetDiscovery(tm.CreateItemType("LogHouse:1"), MetaTime.Now));
			// Re-discover should be a (successful) no-op
			Assert.False(itemDiscovery.SetDiscovery(tm.CreateItemType("LogHouse:1"), MetaTime.Now));

			// LogHouse:1 should be discovered, but not LogHouse:2 or HeroCook:1
			AssertDiscovered(itemDiscovery, "LogHouse:1");
			AssertNotDiscovered(itemDiscovery, "LogHouse:2", "HeroCook:1");

			// There should be reward to claim for LogHouse:1
			Assert.True(itemDiscovery.SomethingToClaim(config));
			Assert.True(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Buildings")));
			Assert.False(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Heroes")));
			AssertCategories(itemDiscovery.SomethingToClaimCategories(config), "Buildings");
			AssertItems(itemDiscovery.NextItems(config), "LogHouse:2");

			// Claim the reward for LogHouse:1
			itemDiscovery.MarkClaimed(tm.CreateItemType("LogHouse:1"), MetaTime.Now);
			AssertClaimed(itemDiscovery, "LogHouse:1");

			// There should be no reward(s) to claim anymore
			Assert.False(itemDiscovery.SomethingToClaim(config));
			Assert.False(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Buildings")));
			CollectionAssert.IsEmpty(itemDiscovery.SomethingToClaimCategories(config));

			// Next items should be unaffected by the claiming of the reward
			AssertItems(itemDiscovery.NextItems(config), "LogHouse:2");
		}

		[Test]
		public void Multiple() {
			ItemDiscovery itemDiscovery = new ItemDiscovery();
			// Discover LogHouse:1 and LogHouse:2
			itemDiscovery.SetDiscovery(tm.CreateItemType("LogHouse:1"), MetaTime.Now);
			itemDiscovery.SetDiscovery(tm.CreateItemType("LogHouse:2"), MetaTime.Now);

			// Discover all OrangeTrees
			for (int i = 1; i <= 5; i++) {
				itemDiscovery.SetDiscovery(
					new LevelId<ChainTypeId>(ChainTypeId.FromString("OrangeTree"), i),
					MetaTime.Now
				);
			}

			// The items should be discovered and there should be rewards to claim in two categories
			AssertDiscovered(
				itemDiscovery,
				"LogHouse:1",
				"LogHouse:2",
				"OrangeTree:1",
				"OrangeTree:2",
				"OrangeTree:3",
				"OrangeTree:4",
				"OrangeTree:5"
			);
			Assert.True(itemDiscovery.SomethingToClaim(config));
			Assert.True(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Buildings")));
			Assert.True(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Misc")));
			AssertCategories(itemDiscovery.SomethingToClaimCategories(config), "Buildings", "Misc");
			// Only LogHouse:3 should be among next items (because OrangeTree:5 is max level)
			AssertItems(itemDiscovery.NextItems(config), "LogHouse:3");

			// Claim a couple of rewards (but leave rewards to be claimed later
			itemDiscovery.MarkClaimed(tm.CreateItemType("LogHouse:1"), MetaTime.Now);
			itemDiscovery.MarkClaimed(tm.CreateItemType("OrangeTree:1"), MetaTime.Now);
			itemDiscovery.MarkClaimed(tm.CreateItemType("OrangeTree:2"), MetaTime.Now);

			// Discovery states should have changed...
			AssertDiscovered(itemDiscovery, "LogHouse:2", "OrangeTree:3", "OrangeTree:4");
			AssertClaimed(itemDiscovery, "LogHouse:1", "OrangeTree:1", "OrangeTree:2");
			// ...but there are still rewards to be claimed in both categories
			Assert.True(itemDiscovery.SomethingToClaim(config));
			Assert.True(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Buildings")));
			Assert.True(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Misc")));
			AssertCategories(itemDiscovery.SomethingToClaimCategories(config), "Buildings", "Misc");

			// Claim last reward in "Buildings" category
			itemDiscovery.MarkClaimed(tm.CreateItemType("LogHouse:2"), MetaTime.Now);
			Assert.True(itemDiscovery.SomethingToClaim(config));
			Assert.False(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Buildings")));
			Assert.True(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Misc")));
			AssertCategories(itemDiscovery.SomethingToClaimCategories(config), "Misc");

			// Claim the rest of the rewards for OrangeTrees
			itemDiscovery.MarkClaimed(tm.CreateItemType("OrangeTree:3"), MetaTime.Now);
			itemDiscovery.MarkClaimed(tm.CreateItemType("OrangeTree:4"), MetaTime.Now);
			itemDiscovery.MarkClaimed(tm.CreateItemType("OrangeTree:5"), MetaTime.Now);
			// There should be no rewards to claim anymore
			Assert.False(itemDiscovery.SomethingToClaim(config));
			Assert.False(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Buildings")));
			Assert.False(itemDiscovery.SomethingToClaimInCategory(config, CategoryId.FromString("Misc")));
			AssertCategories(itemDiscovery.SomethingToClaimCategories(config));
		}

		private void AssertDiscovered(ItemDiscovery itemDiscovery, params string[] items) {
			foreach (var item in items) {
				Assert.AreEqual(
					DiscoveryState.Discovered,
					itemDiscovery.GetState(tm.CreateItemType(item))
				);
			}
		}

		private void AssertNotDiscovered(ItemDiscovery itemDiscovery, params string[] items) {
			foreach (var item in items) {
				Assert.AreEqual(
					DiscoveryState.NotDiscovered,
					itemDiscovery.GetState(tm.CreateItemType(item))
				);
			}
		}

		private void AssertClaimed(ItemDiscovery itemDiscovery, params string[] items) {
			foreach (var item in items) {
				Assert.AreEqual(
					DiscoveryState.Claimed,
					itemDiscovery.GetState(tm.CreateItemType(item))
				);
			}
		}

		private void AssertItems(List<LevelId<ChainTypeId>> actual, params string[] expectedItems) {
			Assert.That(
				actual.Count == expectedItems.Length,
				"Should contain {0} items (but contains {1})",
				expectedItems.Length,
				actual.Count
			);
			foreach (var itemString in expectedItems) {
				CollectionAssert.Contains(actual, tm.CreateItemType(itemString));
			}
		}

		private void AssertCategories(OrderedSet<CategoryId> actual, params string[] expected) {
			Assert.That(
				actual.Count == expected.Length,
				"Should contain {0} categories (but contains {1})",
				expected.Length,
				actual.Count
			);
			foreach (var categoryName in expected) {
				CollectionAssert.Contains(actual, CategoryId.FromString(categoryName));
			}
		}
	}
}
