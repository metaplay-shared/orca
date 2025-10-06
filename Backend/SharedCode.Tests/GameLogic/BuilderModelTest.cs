using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class BuilderModelTest {
		private MetaTime time;
		private BuildersModel builders;

		[SetUp]
		public void StartTest() {
			time = MetaTime.Now;
			builders = new BuildersModel(2);
			builders.AddTemporaryBuilderTime(time, MetaDuration.FromHours(1));
		}

		[Test]
		public void FreeAndTotal() {
			Assert.AreEqual(3, builders.Free);
			Assert.AreEqual(3, builders.Total);

			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			Assert.AreEqual(1, builders.Free);
			Assert.AreEqual(3, builders.Total);

			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			Assert.AreEqual(0, builders.Free);
			Assert.AreEqual(3, builders.Total);

			// Release one builder
			builders.Update(time + MetaDuration.FromHours(1));
			Assert.AreEqual(1, builders.Free);
			// Also the last builders is released
			builders.Update(time + MetaDuration.FromHours(2));
			Assert.AreEqual(3, builders.Free);
		}

		[Test]
		public void OccupiedBuilders() {
			OrderedSet<int> occupied = builders.OccupiedBuilders;
			Assert.AreEqual(0, occupied.Count);

			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			occupied = builders.OccupiedBuilders;
			Assert.AreEqual(1, occupied.Count);
			Assert.Contains(1, occupied);

			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			occupied = builders.OccupiedBuilders;
			Assert.AreEqual(3, occupied.Count);
			Assert.Contains(1, occupied);
			Assert.Contains(2, occupied);
			Assert.Contains(100, occupied);
		}

		[Test]
		public void AssignTask() {
			// 
			int id = builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			Assert.AreEqual(1, id);
			id = builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			Assert.AreEqual(2, id);
			// A task assigned to the temporary builder. Just check that the id is greater than the permanent ones
			// would have
			id = builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			Assert.Greater(id, 10);
			// No more builders available. This should never happen in prod as we do validation in the actions first
			id = builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			Assert.Less(id, 0);
		}

		[Test]
		public void Update() {
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(3));

			int released = builders.Update(time + MetaDuration.FromHours(1));
			Assert.AreEqual(1, released);
			released = builders.Update(time + MetaDuration.FromHours(3));
			Assert.AreEqual(2, released);
			released = builders.Update(time + MetaDuration.FromHours(10));
			Assert.AreEqual(0, released);
		}

		[Test]
		public void CleanupBasicCases() {
			// Nothing is removed in normal case
			builders.Cleanup(2, time);
			Assert.AreEqual(3, builders.Total);

			// The extra permanent builder is removed when idling
			builders.Cleanup(1, time);
			Assert.AreEqual(2, builders.Total);

			// A new permanent builder is added as expected
			builders.Cleanup(2, time);
			Assert.AreEqual(3, builders.Total);
		}

		[Test]
		public void CleanupPermanentBuilders() {
			// Occupied permanent builders are not cleaned up
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.Cleanup(1, time);
			Assert.AreEqual(3, builders.Total);

			// Free permanent builder is cleaned up
			builders.Update(time + MetaDuration.FromHours(2));
			builders.Cleanup(1, time);
			Assert.AreEqual(2, builders.Total);
		}

		[Test]
		public void CleanupTemporaryBuilders() {
			// Occupied temporary builders are not cleaned up
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.Cleanup(2, time + MetaDuration.FromHours(3));
			Assert.AreEqual(3, builders.Total);

			// Free temporary builders are cleaned up
			builders.Update(time + MetaDuration.FromHours(2));
			builders.Cleanup(2, time + MetaDuration.FromHours(3));
			Assert.AreEqual(2, builders.Total);
		}

		[Test]
		public void GetCompleteAt() {
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(3));

			Assert.AreEqual(time + MetaDuration.FromHours(1), builders.GetCompleteAt(1));
			Assert.AreEqual(time + MetaDuration.FromHours(2), builders.GetCompleteAt(2));
			Assert.AreEqual(time + MetaDuration.FromHours(3), builders.GetCompleteAt(100));

			Assert.AreEqual(MetaTime.Epoch, builders.GetCompleteAt(999));
		}

		[Test]
		public void Reset() {
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(1));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(2));
			builders.AssignTask(IslandTypeId.MainIsland, time, MetaDuration.FromHours(3));

			builders.Reset(1);
			builders.Reset(100);
			builders.Reset(999);

			Assert.AreEqual(2, builders.Free);
			Assert.AreEqual(MetaTime.Epoch, builders.GetCompleteAt(1));
			Assert.AreEqual(time + MetaDuration.FromHours(2), builders.GetCompleteAt(2));
			Assert.AreEqual(MetaTime.Epoch, builders.GetCompleteAt(100));
		}

		[Test]
		public void AddMultipleTemporaryBuilders() {
			Assert.AreEqual(1, builders.Temporary.Count); // Builder added by StartTest()
			Assert.NotNull(builders.Temporary[100]);
			// Extend existing builder
			Assert.AreEqual(100, builders.AddTemporaryBuilderTime(time, MetaDuration.FromHours(1), 100));
			Assert.AreEqual(time + MetaDuration.FromHours(2), builders.Temporary[100].ExpiresAt);
			// Create new builder
			Assert.AreEqual(101, builders.AddTemporaryBuilderTime(time, MetaDuration.FromHours(3)));
			Assert.AreEqual(time + MetaDuration.FromHours(2), builders.Temporary[100].ExpiresAt);
			Assert.AreEqual(time + MetaDuration.FromHours(3), builders.Temporary[101].ExpiresAt);
			time += MetaDuration.FromHours(2);
			builders.Cleanup(2, time);
			Assert.False(builders.Temporary.ContainsKey(100));
			Assert.True(builders.Temporary.ContainsKey(101));

			Assert.AreEqual(100, builders.AddTemporaryBuilderTime(time, MetaDuration.FromHours(5), 100));
			Assert.AreEqual(102, builders.AddTemporaryBuilderTime(time, MetaDuration.FromHours(5), 155));
			Assert.AreEqual(3, builders.Temporary.Count);
		}
	}
}
