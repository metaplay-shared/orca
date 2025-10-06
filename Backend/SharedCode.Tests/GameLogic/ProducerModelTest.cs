using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Math;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class ProducerModelTest {
		private ProducerModel model;
		private F64 prodPerHour;

		[SetUp]
		public void StartTest() {
			model = new ProducerModel();
		}

		[Test]
		public void TimeToNext() {
			MetaTime now = MetaTime.Epoch;
			model.Reset(now, 3);
			prodPerHour = F64.FromInt(360); // Production rate: 1 item in 10s

			// TimeToNext should be zero when producer is full or "over full".
			Assert.That(TimeToNextMs(3, now), Is.Zero);
			Assert.That(TimeToNextMs(2, now), Is.Zero);

			// 1 ms to the next item (take account the rounding)
			Assert.That(TimeToNextMs(4, now + new MetaDuration(9999)), Is.InRange(1, 2));

			// After one "production cycle" TimeToNext should be zero
			Assert.That(TimeToNextMs(4, now + new MetaDuration(10000)), Is.Zero);
			Assert.That(TimeToNextMs(4, now + new MetaDuration(10001)), Is.Zero);

			Assert.That(TimeToNextMs(4, now + new MetaDuration(4300)), Is.InRange(5699, 5701));
		}

		[Test]
		public void TimeToFill() {
			MetaTime now = MetaTime.Epoch;
			model.Reset(now, 3);
			prodPerHour = F64.FromInt(360); // Production rate: 1 item in 10s

			// Full producer needs no time to fill
			Assert.That(TimeToFillMs(3, now), Is.Zero);
			Assert.That(TimeToFillMs(2, now), Is.Zero);

			Assert.That(TimeToFillMs(4, now), Is.EqualTo(10000));
			Assert.That(TimeToFillMs(4, now + new MetaDuration(9500)), Is.EqualTo(500));
			Assert.That(TimeToFillMs(5, now), Is.EqualTo(20000));
		}

		[Test]
		public void Update() {
			MetaTime startTime = MetaTime.Epoch;
			int maxItems = 3;
			model.Reset(startTime, maxItems);
			prodPerHour = F64.FromInt(360); // Production rate: 1 item in 10s

			// Initial state
			Assert.AreEqual(startTime, model.LastUpdated);
			Assert.AreEqual(maxItems, model.ProducedAtUpdate);

			// Updating with the same or earlier timestamp should not modify the model.
			model.Update(startTime, prodPerHour, maxItems);
			Assert.AreEqual(startTime, model.LastUpdated);
			Assert.AreEqual(maxItems, model.ProducedAtUpdate);
			model.Update(startTime - new MetaDuration(15000), prodPerHour, maxItems);
			Assert.AreEqual(startTime, model.LastUpdated);
			Assert.AreEqual(maxItems, model.ProducedAtUpdate);

			// Updating with an earlier timestamp should not modify the model when producer is not full.
			model.Consume(2);
			Assert.AreEqual(1, model.ProducedAtUpdate);
			model.Update(startTime - new MetaDuration(15000), prodPerHour, maxItems);
			Assert.AreEqual(startTime, model.LastUpdated);
			Assert.AreEqual(1, model.ProducedAtUpdate);
			model.Add(2);
			Assert.AreEqual(maxItems, model.ProducedAtUpdate);

			// LastUpdated is updated to 'now' when producer is full.
			MetaTime now = startTime + new MetaDuration(1000);
			model.Update(now, prodPerHour, maxItems);
			Assert.AreEqual(now, model.LastUpdated);
			Assert.AreEqual(maxItems, model.ProducedAtUpdate);

			// 1 ms before production is finished
			model.Consume(1);
			now += new MetaDuration(9999);
			model.Update(now, prodPerHour, maxItems);
			Assert.AreEqual(now - new MetaDuration(9999), model.LastUpdated);
			Assert.AreEqual(2, model.ProducedAtUpdate);

			// Production finished (producer full again)
			now += new MetaDuration(1);
			model.Update(now, prodPerHour, maxItems);
			Assert.AreEqual(now, model.LastUpdated);
			Assert.AreEqual(maxItems, model.ProducedAtUpdate);

			// Progress over one production cycle
			model.Consume(2);
			now += new MetaDuration(16000);
			model.Update(now, prodPerHour, maxItems);
			Assert.AreEqual(now + new MetaDuration(-6000), model.LastUpdated);
			Assert.AreEqual(2, model.ProducedAtUpdate);
		}

		private long TimeToFillMs(int maxItems, MetaTime currentTime) {
			return model.TimeToFill(prodPerHour, maxItems, currentTime).Milliseconds;
		}

		private long TimeToNextMs(int maxItems, MetaTime currentTime) {
			return model.TimeToNext(currentTime, prodPerHour, maxItems).Milliseconds;
		}
	}
}
