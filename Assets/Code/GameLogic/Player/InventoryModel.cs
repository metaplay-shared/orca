using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class InventoryModel {
		[MetaMember(1)] public MetaDictionary<CurrencyTypeId, int> Resources { get; private set; }
		[MetaMember(2)] public OrderedSet<ChainTypeId> UnlockedResourceItems { get; private set; }
		[MetaMember(3)] public OrderedSet<ChainTypeId> UnlockedResourceCreators { get; private set; }

		public InventoryModel() { }

		public InventoryModel(SharedGameConfig gameConfig) {
			Resources = new MetaDictionary<CurrencyTypeId, int>();
			// foreach (ResourceInfo resource in gameConfig.Global.InitialResources) {
			// 	Resources[resource.Type] = resource.Amount;
			// }

			UnlockedResourceItems = new OrderedSet<ChainTypeId>();
			UnlockedResourceCreators = new OrderedSet<ChainTypeId>();
		}

		public bool HasEnoughResources(HeroTaskInfo task) {
			foreach (ResourceInfo resource in task.Resources) {
				if (Resources.GetValueOrDefault(resource.Type) < resource.Amount) {
					return false;
				}
			}

			return true;
		}

		public void ModifyResources(CurrencyTypeId type, int delta) {
			Resources[type] = Resources.GetValueOrDefault(type) + delta;
		}

		public void UnlockResourceItem(ChainTypeId item) {
			UnlockedResourceItems.Add(item);
		}

		public void UnlockResourceCreator(ChainTypeId creator) {
			UnlockedResourceCreators.Add(creator);
		}

		public ChainTypeId GetRandomUnlockedResourceItem(SharedGameConfig gameConfig, RandomPCG random, IslandTypeId island) {
			return RandomizeUnlocked(gameConfig, random, island, UnlockedResourceItems);
		}

		public ChainTypeId GetRandomUnlockedResourceCreator(SharedGameConfig gameConfig, RandomPCG random, IslandTypeId island) {
			return RandomizeUnlocked(gameConfig, random, island, UnlockedResourceCreators);
		}

		private ChainTypeId RandomizeUnlocked(SharedGameConfig gameConfig, RandomPCG random, IslandTypeId island, OrderedSet<ChainTypeId> unlocked) {
			List<ChainTypeId> types = new();
			foreach (ChainTypeId item in unlocked) {
				ChainInfo info = gameConfig.Chains[new LevelId<ChainTypeId>(item, 1)];
				IslandInfo islandInfo = gameConfig.Islands[island];
				if (islandInfo.GenerateAllResources || info.TargetIsland == IslandTypeId.All || info.TargetIsland == island) {
					types.Add(item);
				}
			}

			if (types.Count > 0) {
				return types[random.NextInt(types.Count)];
			}

			return ChainTypeId.None;
		}
	}
}
