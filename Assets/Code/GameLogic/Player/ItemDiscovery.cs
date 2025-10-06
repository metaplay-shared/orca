using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class DiscoveryStatus {
		[MetaMember(1)] public DiscoveryState State { get; private set; }
		[MetaMember(2)] public MetaTime DiscoveryTime { get; private set; }
		[MetaMember(3)] public MetaTime ClaimTime { get; private set; }
		[MetaMember(4)] public int Count { get; set; }

		public DiscoveryStatus() { }

		public DiscoveryStatus(MetaTime discoveryTime) {
			State = DiscoveryState.Discovered;
			DiscoveryTime = discoveryTime;
			ClaimTime = MetaTime.Epoch;
			Count = 1;
		}

		public void MarkClaimed(MetaTime currentTime) {
			State = DiscoveryState.Claimed;
			ClaimTime = currentTime;
		}
	}

	[MetaSerializable]
	public enum DiscoveryState {
		NotDiscovered,
		Discovered,
		Claimed
	}

	public interface IDiscoveryHandler {
		DiscoveryState GetState(LevelId<ChainTypeId> key);
	}

	[MetaSerializable]
	public class ItemDiscovery : IDiscoveryHandler {
		[MetaMember(1)] public MetaDictionary<LevelId<ChainTypeId>, DiscoveryStatus> Discovery { get; private set; } = new();

		public bool SomethingToClaim(SharedGameConfig gameConfig) {
			foreach (var entry in Discovery) {
				if (entry.Value.State == DiscoveryState.Discovered) {
					ChainInfo chainInfo = gameConfig.Chains[entry.Key];
					if (gameConfig.Global.DiscoveryCategories.Contains(chainInfo.Category)) {
						return true;
					}
				}
			}

			return false;
		}

		public bool SomethingToClaimInCategory(SharedGameConfig gameConfig, CategoryId category) {
			foreach (var entry in Discovery) {
				if (entry.Value.State == DiscoveryState.Discovered) {
					ChainInfo chainInfo = gameConfig.Chains[entry.Key];
					if (chainInfo.Category == category) {
						return true;
					}
				}
			}

			return false;
		}

		public OrderedSet<CategoryId> SomethingToClaimCategories(SharedGameConfig gameConfig) {
			OrderedSet<CategoryId> categories = new();
			foreach (var entry in Discovery) {
				if (entry.Value.State == DiscoveryState.Discovered) {
					ChainInfo chainInfo = gameConfig.Chains[entry.Key];
					CategoryId categoryId = chainInfo.Category;
					if (gameConfig.Global.DiscoveryCategories.Contains(categoryId)) {
						categories.Add(categoryId);
					}
				}
			}

			return categories;
		}

		public List<LevelId<ChainTypeId>> NextItems(SharedGameConfig gameConfig) {
            MetaDictionary<ChainTypeId, int> maxFoundLevels = new();
			foreach (LevelId<ChainTypeId> item in Discovery.Keys) {
				int maxFoundLevel = maxFoundLevels.GetValueOrDefault(item.Type);
				if (item.Level > maxFoundLevel) {
					maxFoundLevels[item.Type] = item.Level;
				}
			}

			List<LevelId<ChainTypeId>> items = new();
			foreach (var entry in maxFoundLevels) {
				int maxLevel = gameConfig.ChainMaxLevels.GetMaxLevel(entry.Key);
				if (maxLevel > entry.Value) {
					items.Add(new LevelId<ChainTypeId>(entry.Key, entry.Value + 1));
				}
			}

			return items;
		}

		public bool SetDiscovery(LevelId<ChainTypeId> chainId, MetaTime currentTime) {
			if (Discovery.ContainsKey(chainId)) {
				Discovery[chainId].Count++;
			} else {
				Discovery[chainId] = new DiscoveryStatus(currentTime);
				return true;
			}

			return false;
		}

		public void MarkClaimed(LevelId<ChainTypeId> chainId, MetaTime currentTime) {
			Discovery[chainId].MarkClaimed(currentTime);
		}

		public DiscoveryState GetState(LevelId<ChainTypeId> chainId) {
			if (Discovery.ContainsKey(chainId)) {
				return Discovery[chainId].State;
			}

			return DiscoveryState.NotDiscovered;
		}

		public int ItemCount(LevelId<ChainTypeId> chainId) {
			if (Discovery.ContainsKey(chainId)) {
				return Discovery[chainId].Count;
			}

			return 0;
		}
	}
}
