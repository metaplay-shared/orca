using System;
using System.Collections.Generic;
using System.Linq;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class HeroesModel {
		[MetaMember(1)] public MetaDictionary<HeroTypeId, HeroModel> Heroes { get; private set; }
		[MetaMember(2)] public HeroTypeId CurrentHero { get; private set; }
		[MetaMember(3)] public ChainTypeId CurrentHeroItem { get; private set; }
		[MetaMember(4)] public AssignHeroModel AssignHero { get; private set; }

		public HeroesModel() { }

		public HeroesModel(SharedGameConfig gameConfig) {
			Heroes = new MetaDictionary<HeroTypeId, HeroModel>();
			foreach (HeroInfo heroInfo in gameConfig.Heroes.Values) {
				if (heroInfo.Index == 1) {
					CurrentHero = heroInfo.Type;
					CurrentHeroItem = heroInfo.ItemType;
					break;
				}
			}

			AssignHero = new AssignHeroModel();
		}

		public bool HasInteractableHeroTasks(InventoryModel inventory) {
			foreach (HeroModel hero in Heroes.Values) {
				HeroTaskModel task = hero.CurrentTask;
				if (task != null && task.State == HeroTaskState.Created && inventory.HasEnoughResources(task.Info)) {
					return true;
				}

				if (task != null && task.State == HeroTaskState.Finished) {
					return true;
				}
			}

			return false;
		}

		public List<HeroModel> HeroesInBuilding(ChainTypeId buildingId) {
			return Heroes.Values.Where(h => h.Building == buildingId && h.CurrentTask != null).ToList();
		}

		public bool UnlocksHero(SharedGameConfig gameConfig, ItemModel item) {
			if (item.Info.Type == CurrentHeroItem) {
				return !gameConfig.Chains.ContainsKey(new LevelId<ChainTypeId>(item.Info.Type, item.Info.Level + 1));
			}

			return false;
		}

		public void UnlockHero(SharedGameConfig gameConfig, Action<RewardModel> rewardHandler, IPlayerModelClientListener listener, MetaTime currentTime) {
			HeroInfo heroInfo = gameConfig.Heroes[CurrentHero];
			Heroes[CurrentHero] = new HeroModel(
				heroInfo,
				currentTime
			);
			HeroLevelInfo levelInfo = gameConfig.HeroLevels[new LevelId<HeroTypeId>(CurrentHero, 1)];
			if (levelInfo.RewardItems.Count > 0 || levelInfo.RewardResources.Count > 0) {
				RewardMetadata metadata = new RewardMetadata {
					Type = RewardType.HeroUnlock,
					Level = 1,
					Hero = CurrentHero
				};

				RewardModel rewards = new RewardModel(
					levelInfo.RewardResources,
					levelInfo.RewardItems,
					ChainTypeId.LevelUpRewards,
					1,
					metadata
				);
				rewardHandler.Invoke(rewards);
			}

			AssignBuilding(gameConfig, Heroes[CurrentHero]);

			listener.OnHeroUnlocked(CurrentHero);
			HeroInfo nextHero = FindNextHero(gameConfig, heroInfo.Index);
			if (nextHero == null) {
				CurrentHero = HeroTypeId.None;
				CurrentHeroItem = ChainTypeId.None;
			} else {
				CurrentHero = nextHero.Type;
				CurrentHeroItem = nextHero.ItemType;
				listener.OnNewHeroStarted(CurrentHero);
			}
		}

		public void Update(SharedGameConfig gameConfig, int playerLevel, OrderedSet<ChainTypeId> unlockedResources, MetaTime currentTime, IPlayerModelClientListener listener) {
			foreach (HeroModel hero in Heroes.Values) {
				hero.Update(gameConfig, playerLevel, unlockedResources, currentTime, listener);
			}
		}

		private void AssignBuilding(SharedGameConfig gameConfig, HeroModel newHero) {
            MetaDictionary<ChainTypeId, int> buildingOccupancies = new MetaDictionary<ChainTypeId, int>();
			foreach (HeroModel hero in Heroes.Values) {
				buildingOccupancies[hero.Building] = buildingOccupancies.GetValueOrDefault(hero.Building) + 1;
			}

			int defaultBuildingOccupancy = buildingOccupancies.GetValueOrDefault(newHero.Info.DefaultBuilding);
			if (defaultBuildingOccupancy < gameConfig.Global.MaxHeroesInBuilding) {
				newHero.AssignToBuilding(newHero.Info.DefaultBuilding);
			} else {
				IEnumerable<ChainTypeId> allHeroBuildings =
					gameConfig.Chains.Values.Where(c => c.HeroTarget).Select(c => c.Type);
				foreach (ChainTypeId heroBuilding in allHeroBuildings) {
					int buildingOccupancy = buildingOccupancies.GetValueOrDefault(heroBuilding);
					if (buildingOccupancy < gameConfig.Global.MaxHeroesInBuilding) {
						newHero.AssignToBuilding(heroBuilding);
					}
				}
			}
		}

		private HeroInfo FindNextHero(SharedGameConfig gameConfig, int currentIndex) {
			foreach (HeroInfo heroInfo in gameConfig.Heroes.Values) {
				if (heroInfo.Index == currentIndex + 1) {
					return heroInfo;
				}
			}

			return null;
		}
	}
}
