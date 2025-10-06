using System;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class BuilderModel {
		[MetaMember(1)] public int Id { get; private set; }
		[MetaMember(2)] public MetaTime ExpiresAt { get; private set; }
		[MetaMember(3)] public IslandTypeId Island { get; internal set; }
		[MetaMember(4)] public MetaTime StartedAt { get; internal set; }
		[MetaMember(5)] public MetaTime CompleteAt { get; internal set; }

		public bool IsFree => Island == IslandTypeId.None;
		public MetaDuration TotalTime => CompleteAt - StartedAt;

		public bool IsComplete(MetaTime currentTime) {
			return !IsFree && CompleteAt <= currentTime;
		}

		public BuilderModel() { }

		public BuilderModel(int id) {
			Id = id;
			Island = IslandTypeId.None;
			StartedAt = MetaTime.Epoch;
			CompleteAt = MetaTime.Epoch;
			ExpiresAt = MetaTime.Epoch;
		}

		public BuilderModel(int id, MetaTime expiresAt) : this(id) {
			ExpiresAt = expiresAt;
		}

		public void AssignTask(IslandTypeId island, MetaTime currentTime, MetaDuration duration) {
			Island = island;
			StartedAt = currentTime;
			CompleteAt = currentTime + duration;
		}

		public void Reset() {
			Island = IslandTypeId.None;
			StartedAt = MetaTime.Epoch;
			CompleteAt = MetaTime.Epoch;
		}

		public void ExtendExpiration(MetaDuration duration) {
			ExpiresAt += duration;
		}
	}

	[MetaSerializable]
	public class BuildersModel {
		private const int TEMPORARY_INDEX_OFFSET = 100;
		private const int CONSUMABLE_INDEX_OFFSET = 1000;
		[MetaMember(1)] public MetaDictionary<int, BuilderModel> Permanent { get; private set; }
		[MetaMember(2)] public MetaDictionary<int, BuilderModel> Temporary { get; private set; }
		[MetaMember(3)] public MetaDictionary<int, BuilderModel> Consumable { get; private set; }

		public BuildersModel() { }

		public BuildersModel(int count) {
			Permanent = new MetaDictionary<int, BuilderModel>();
			Temporary = new MetaDictionary<int, BuilderModel>();
			Consumable = new MetaDictionary<int, BuilderModel>();
			for (int i = 0; i < count; i++) {
				Permanent[i + 1] = new BuilderModel(i + 1);
			}
		}

		/// <summary>
		/// <c>AddTemporaryBuilderTime</c> adds the given <paramref name="duration"/> to the expiration time of
		/// an existing builder or creates a new builder with the given expiration time
		/// (<paramref name="currentTime"/> + <paramref name="duration"/>).
		/// </summary>
		/// <param name="currentTime">current time</param>
		/// <param name="duration">duration after which the temporary builder expires</param>
		/// <param name="existingBuilderId">ID of an existing builder whose expiration time to extend. Use <c>-1</c>
		/// if a new temporary builder is to be created.</param>
		/// <returns>ID of the builder whose expiration time was modified. NOTE! This might not be equal to
		/// <paramref name="existingBuilderId"/> if there was no such builder present.</returns>
		public int AddTemporaryBuilderTime(MetaTime currentTime, MetaDuration duration, int existingBuilderId = -1) {
			BuilderModel builder;
			if (existingBuilderId >= TEMPORARY_INDEX_OFFSET && Temporary.ContainsKey(existingBuilderId)) {
				builder = Temporary[existingBuilderId];
				builder.ExtendExpiration(duration);
			} else {
				int freeBuilderId = TEMPORARY_INDEX_OFFSET;
				while (Temporary.ContainsKey(freeBuilderId)) {
					freeBuilderId++;
				}
				builder = new BuilderModel(freeBuilderId, currentTime + duration);
				Temporary[builder.Id] = builder;
			}

			return builder.Id;
		}

		public int AssignTask(IslandTypeId island, MetaTime currentTime, MetaDuration duration) {
			foreach (BuilderModel builder in Permanent.Values) {
				if (builder.IsFree) {
					builder.AssignTask(island, currentTime, duration);
					return builder.Id;
				}
			}

			foreach (BuilderModel builder in Temporary.Values) {
				if (builder.IsFree) {
					builder.AssignTask(island, currentTime, duration);
					return builder.Id;
				}
			}

			return -1;
		}

		public int AssignTaskToConsumable(IslandTypeId island, MetaTime currentTime, MetaDuration duration) {
			int id = Math.Max(FindMaxId(Consumable), CONSUMABLE_INDEX_OFFSET) + 1;
			BuilderModel builder = new BuilderModel(id, currentTime + duration);
			Consumable[id] = builder;
			builder.AssignTask(island, currentTime, duration);
			return id;
		}

		public int Update(MetaTime currentTime) {
			int released = 0;
			foreach (BuilderModel builder in Permanent.Values) {
				if (builder.IsComplete(currentTime)) {
					builder.Reset();
					released++;
				}
			}

			foreach (BuilderModel builder in Temporary.Values) {
				if (builder.IsComplete(currentTime)) {
					builder.Reset();
					released++;
				}
			}

			foreach (BuilderModel builder in Consumable.Values) {
				if (builder.IsComplete(currentTime)) {
					builder.Reset();
					released++;
				}
			}

			return released;
		}

		public void Cleanup(int count, MetaTime currentTime) {
			Cleanup(Temporary, currentTime, false);
			Cleanup(Consumable, currentTime, true);

			for (int i = Permanent.Count; i < count; i++) {
				Permanent[i + 1] = new BuilderModel(i + 1);
			}

			if (Permanent.Count > count) {
				// Try to remove the last permanent builder if it is possible to do so. This will eventually remove
				// the builders that the player should not have. Lowering the count is very unlikely to happen in
				// production and it doesn't matter if the players can use the extra builder a bit longer than expected.
				if (Permanent[Permanent.Count].IsFree) {
					Permanent.Remove(Permanent.Count);
				}
			}
		}

		private void Cleanup(MetaDictionary<int, BuilderModel> builders, MetaTime currentTime, bool consumable) {
			OrderedSet<int> removed = new OrderedSet<int>();
			foreach (BuilderModel builder in builders.Values) {
				if ((consumable || builder.ExpiresAt <= currentTime) && builder.IsFree) {
					removed.Add(builder.Id);
				}
			}

			foreach (int id in removed) {
				builders.Remove(id);
			}
		}

		public int Free {
			get {
				int free = 0;
				foreach (BuilderModel builder in Permanent.Values) {
					if (builder.IsFree) {
						free++;
					}
				}

				foreach (BuilderModel builder in Temporary.Values) {
					if (builder.IsFree) {
						free++;
					}
				}

				return free;
			}
		}

		public int Total => Permanent.Count + Temporary.Count;
		public OrderedSet<int> OccupiedBuilders {
			get {
				OrderedSet<int> occupied = new();
				foreach (BuilderModel builder in Permanent.Values) {
					if (!builder.IsFree) {
						occupied.Add(builder.Id);
					}
				}

				foreach (BuilderModel builder in Temporary.Values) {
					if (!builder.IsFree) {
						occupied.Add(builder.Id);
					}
				}

				foreach (BuilderModel builder in Consumable.Values) {
					if (!builder.IsFree) {
						occupied.Add(builder.Id);
					}
				}

				return occupied;
			}
		}

		public BuilderModel GetBuilder(int builderId) {
			if (Permanent.ContainsKey(builderId)) {
				return Permanent[builderId];
			} else if (Temporary.ContainsKey(builderId)) {
				return Temporary[builderId];
			} else if (Consumable.ContainsKey(builderId)) {
				return Consumable[builderId];
			}

			return null;
		}

		public void SetCompleteAt(int builderId, MetaTime completeAt) {
			if (Permanent.ContainsKey(builderId)) {
				Permanent[builderId].CompleteAt = completeAt;
			} else if (Temporary.ContainsKey(builderId)) {
				Temporary[builderId].CompleteAt = completeAt;
			} else if (Consumable.ContainsKey(builderId)) {
				Consumable[builderId].CompleteAt = completeAt;
			}
		}

		public MetaTime GetCompleteAt(int builderId) {
			if (Permanent.ContainsKey(builderId)) {
				return Permanent[builderId].CompleteAt;
			}
			if (Temporary.ContainsKey(builderId)) {
				return Temporary[builderId].CompleteAt;
			}
			if (Consumable.ContainsKey(builderId)) {
				return Consumable[builderId].CompleteAt;
			}
			return MetaTime.Epoch;
		}

		public MetaDuration GetTotalTime(int builderId) {
			if (Permanent.ContainsKey(builderId)) {
				return Permanent[builderId].TotalTime;
			}
			if (Temporary.ContainsKey(builderId)) {
				return Temporary[builderId].TotalTime;
			}
			if (Consumable.ContainsKey(builderId)) {
				return Consumable[builderId].TotalTime;
			}
			return MetaDuration.Zero;
		}

		public void Reset(int builderId) {
			if (Permanent.ContainsKey(builderId)) {
				Permanent[builderId].Reset();
			} else if (Temporary.ContainsKey(builderId)) {
				Temporary[builderId].Reset();
			} else if (Consumable.ContainsKey(builderId)) {
				Consumable[builderId].Reset();
			}
		}

		private int FindMaxId(MetaDictionary<int, BuilderModel> builders) {
			int max = 0;
			foreach (int id in builders.Keys) {
				max = Math.Max(id, max);
			}

			return max;
		}
	}
}
