using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class IslandModel {
		[MetaMember(1)] public IslandInfo Info { get; private set; }
		[MetaMember(2)] public MergeBoardModel MergeBoard { get; private set; }
		[MetaMember(3)] public IslandTasksModel Tasks { get; private set; }
		[MetaMember(4)] public IslandState State { get; private set; }
		[MetaMember(5)] public MetaDictionary<int, bool> CompletedBuildingSlots { get; private set; }
		[MetaMember(6)] public IslandLevelModel IslandLevel { get; private set; }
		[MetaMember(7)] public BuildingLevelModel BuildingLevel { get; private set; }
		[MetaMember(8)] public MetaTime BuildingDailyRewardClaimedAt { get; private set; }
		[MetaMember(9)] public BuildingState BuildingState { get; private set; }

		public IslandModel() { }

		public IslandModel(
			IslandInfo info,
			SharedGameConfig gameConfig,
			MetaTime currentTime,
			Action<ItemModel> discoveryHandler
		) {
			Info = info;
			if (info.Type == IslandTypeId.MainIsland) {
				State = IslandState.Open;
				BuildingState = BuildingState.None;
				MergeBoard = new MergeBoardModel(gameConfig, Info.Type, currentTime, discoveryHandler);
			} else {
				Tasks = new IslandTasksModel(currentTime);
				State = IslandState.Hidden;
				BuildingState = gameConfig.IslandBuildingFragments[info.Type].Count > 0 ? BuildingState.NotRevealed : BuildingState.None;
			}

			CompletedBuildingSlots = new MetaDictionary<int, bool>();
			IslandLevel = new IslandLevelModel(Info.Type);
			BuildingLevel = new BuildingLevelModel(Info.Type);
			BuildingDailyRewardClaimedAt = MetaTime.Epoch;
		}

		public void ModifyState(
			IslandState newState,
			SharedGameConfig gameConfig,
			MetaTime currentTime,
			Action<ItemModel> discoveryHandler,
			IPlayerModelClientListener listener
		) {
			IslandState oldState = State;
			State = newState;
			if (oldState != State) {
				if (State == IslandState.Open && MergeBoard == null) {
					InitMergeBoard(gameConfig, currentTime, discoveryHandler);
				}

				listener.OnIslandStateModified(Info.Type);
			}
		}

		public void InitMergeBoard(SharedGameConfig gameConfig, MetaTime currentTime, Action<ItemModel> discoveryHandler) {
			MergeBoard = new MergeBoardModel(gameConfig, Info.Type, currentTime, discoveryHandler);
		}

		public bool BuildingSlotsComplete(SharedGameConfig gameConfig) {
			return CompletedBuildingSlots.Count > 0 && CompletedBuildingSlots.Count >= gameConfig.IslandBuildingFragments[Info.Type].Count;
		}

		public bool BuildingDailyRewardAvailable(SharedGameConfig gameConfig, MetaTime currentTime) {
			return BuildingSlotsComplete(gameConfig) && currentTime - BuildingDailyRewardClaimedAt >= gameConfig.Global.BuildingDailyRewardInterval;
		}

		public void MarkBuildingDailyRewardClaimed(MetaTime currentTime) {
			BuildingDailyRewardClaimedAt = currentTime;
		}

		public MetaDuration TimeToDailyRewards(SharedGameConfig gameConfig, MetaTime currentTime) {
			MetaTime nextTime = BuildingDailyRewardClaimedAt + gameConfig.Global.BuildingDailyRewardInterval;
			if (nextTime > currentTime) {
				return nextTime - currentTime;
			}

			return MetaDuration.Zero;
		}

		public bool HasItemsToCollect(SharedGameConfig gameConfig) {
			return MergeBoard?.ItemCount(
					type: null,
					level: 0,
					includeLockedAreas: false,
					item => item.CanCollect(Info.ConfigKey) &&
						item.Info.Level == gameConfig.ChainMaxLevels.GetMaxLevel(item.Info.Type)
				) > 0;
		}

		public Cost SkipCreatorTimerCost(SharedGameConfig gameConfig, MetaTime currentTime) {
			TimerCostInfo costInfo = gameConfig.TimerCosts[TimerTypeId.SkipBuildingDailyTimer];
			int secondsLeft = F64.CeilToInt(TimeToDailyRewards(gameConfig, currentTime).ToSecondsF64());
			return new Cost(costInfo.CurrencyType, costInfo.CalculateCost(secondsLeft));
		}

		public int MarkBuildingFragmentDone(SharedGameConfig gameConfig, ChainTypeId type) {
			for (int i = 0; i < gameConfig.IslandBuildingFragments[Info.Type].Count; i++) {
				ChainTypeId fragment = gameConfig.IslandBuildingFragments[Info.Type][i];
				if (fragment == type && !CompletedBuildingSlots.GetValueOrDefault(i)) {
					CompletedBuildingSlots[i] = true;
					return i;
				}
			}

			return -1;
		}

		public bool IsCompleteBuildingFragment(SharedGameConfig gameConfig, ItemModel item) {
			int maxLevel = gameConfig.ChainMaxLevels.GetMaxLevel(item.Info.Type);
			if (item.Info.Level == maxLevel) {
				return gameConfig.IslandBuildingFragments[Info.Type].Contains(item.Info.Type);
			}

			return false;
		}

		public MergeBoardModel.MoveResultType MoveResult(SharedGameConfig gameConfig, ItemModel item, int x, int y) {
			MergeBoardModel.MoveResultType result = MergeBoard.MoveResult(gameConfig, item, x, y);
			if (result == MergeBoardModel.MoveResultType.Move && IsTargetBuilding(x, y)) {
				return CanClaimAsBuilding(gameConfig, item)
					? MergeBoardModel.MoveResultType.Building
					: MergeBoardModel.MoveResultType.Invalid;
			}
			return result;
		}

		public bool IsTargetBuilding(int toX, int toY) {
			var tile = MergeBoard[toX, toY];
			if (tile == null) {
				return false;
			}

			return tile.HasItem && tile.Item.Info.Building;
		}

		public bool CanClaimAsBuilding(SharedGameConfig gameConfig, ItemModel item) {
			if (item.Info.Type == ChainTypeId.None) {
				return false;
			}

			if (item.BuildState != ItemBuildState.Complete) {
				return false;
			}

			if (BuildingSlotsComplete(gameConfig)) {
				return gameConfig.IslandBuildingFragments[Info.Type].Contains(item.Info.Type);
			}

			int maxLevel = gameConfig.ChainMaxLevels.GetMaxLevel(item.Info.Type);
			if (item.Info.Level == maxLevel) {
				int unfinishedCount = GetUnfinishedFragmentCount(gameConfig, item.Info.Type);
				return unfinishedCount > 0;
			}

			return false;
		}

		public void UseBuildingFragment(
			SharedGameConfig gameConfig,
			ChainInfo itemInfo,
			Action<RewardModel> rewardHandler,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener) {
			int value = itemInfo.CollectableValue;
			BuildingLevel.AddXp(gameConfig, value, rewardHandler, listener, serverListener, ResourceModificationContext.Empty);
		}

		public void AddIslandXp(
			SharedGameConfig gameConfig,
			int delta,
            Action<RewardModel> rewardHandler,
            IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener) {
			IslandLevel.AddXp(gameConfig, delta, rewardHandler, listener, serverListener, ResourceModificationContext.Empty);
		}

		public void UpdateBuildingState(SharedGameConfig gameConfig, Action<IslandTypeId> buildingStateHandler, IPlayerModelClientListener listener) {
			if (BuildingState == BuildingState.NotRevealed) {
				if (MergeBoard.BuildingRevealed) {
					BuildingState = BuildingState.Revealed;
					listener.OnBuildingRevealed(Info.Type);
					buildingStateHandler.Invoke(Info.Type);
				}
			} else if (BuildingState != BuildingState.Complete) {
				if (BuildingSlotsComplete(gameConfig)) {
					BuildingState = BuildingState.Complete;
					listener.OnBuildingCompleted(Info.Type);
					buildingStateHandler.Invoke(Info.Type);
				}
			}
		}

		public void UpdateBuildingStartState(SharedGameConfig gameConfig, Action<IslandTypeId> buildingStateHandler) {
			// Handle this in a separate method since UpdateBuildingState is called very frequently on state Revealed
			// and the operation below is somewhat heavy.
			if (BuildingState == BuildingState.Revealed) {
				OrderedSet<ChainTypeId> fragments = new OrderedSet<ChainTypeId>(gameConfig.IslandBuildingFragments[Info.Type]);
				foreach (ChainTypeId fragment in fragments) {
					if (MergeBoard.ItemCount(
						    fragment,
						    gameConfig.ChainMaxLevels.GetMaxLevel(fragment),
						    includeLockedAreas: false,
						    item => item.CanMove) > 0) {
						BuildingState = BuildingState.Started;
						buildingStateHandler.Invoke(Info.Type);
					}
				}
			}
		}

		public void OpenLockArea(
			SharedGameConfig gameConfig,
			char areaIndex,
			Action<ItemModel> discoveryHandler,
			Action<IslandTypeId> buildingStateHandler,
			IPlayerModelClientListener listener
		) {
			MergeBoard.LockArea.OpenArea(areaIndex);
			MergeBoard.CalculateItemStates(discoveryHandler, listener);
			UpdateBuildingState(gameConfig, buildingStateHandler, listener);
			listener.OnLockAreaOpened(Info.Type, areaIndex);
		}

		public int GetUnfinishedFragmentCount(SharedGameConfig gameConfig, ChainTypeId type) {
			int totalCount = 0;
			for (int i = 0; i < gameConfig.IslandBuildingFragments[Info.Type].Count; i++) {
				ChainTypeId fragment = gameConfig.IslandBuildingFragments[Info.Type][i];
				bool complete = CompletedBuildingSlots.GetValueOrDefault(i);
				if (fragment == type && !complete) {
					totalCount++;
				}
			}

			return totalCount;
		}

		public void RunIslandTaskTriggers(Action<TriggerId> triggerHandler) {
			if (Tasks == null) {
				return;
			}
			foreach (IslandTaskModel task in Tasks.Tasks.Values) {
				if (task.Enabled && task.Info.ItemsAvailableTriggers.Count > 0 && MergeBoard.HasItemsForTask(task.Info)) {
					foreach (TriggerId trigger in task.Info.ItemsAvailableTriggers) {
						triggerHandler.Invoke(trigger);
					}
				}
			}
		}
	}

	[MetaSerializable]
	public enum IslandState {
		Hidden,
		Revealing,
		Locked,
		Open
	}

	[MetaSerializable]
	public enum BuildingState {
		None,
		NotRevealed,
		Revealed,
		Started,
		Complete
	}
}
