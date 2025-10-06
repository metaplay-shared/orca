using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimHeroTaskRewards)]
	public class PlayerClaimHeroTaskRewards : PlayerAction {
		public HeroTypeId HeroType { get; private set; }

		public PlayerClaimHeroTaskRewards() { }

		public PlayerClaimHeroTaskRewards(HeroTypeId heroType) {
			HeroType = heroType;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Heroes.Heroes.ContainsKey(HeroType)) {
				return ActionResult.InvalidParam;
			}

			HeroModel hero = player.Heroes.Heroes[HeroType];
			if (hero.CurrentTask == null) {
				return ActionResult.InvalidState;
			}

			if (hero.CurrentTask.State != HeroTaskState.Finished) {
				return ActionResult.InvalidState;
			}

			MergeBoardModel mergeBoard = player.Islands[IslandTypeId.MainIsland].MergeBoard;
			ItemModel heroBuilding = mergeBoard.FindItem(i => i.Info.Type == hero.Building);
			if (heroBuilding == null) {
				return ActionResult.InvalidState;
			}

			Coordinates coordinates = mergeBoard.FindClosestFreeTile(heroBuilding.X, heroBuilding.Y);
			if (coordinates == null) {
				return ActionResult.NotEnoughSpace;
			}

			if (commit) {
				ItemModel reward = new ItemModel(
					hero.CurrentTask.Info.ItemType,
					1,
					player.GameConfig,
					player.CurrentTime,
					true
				);
				foreach (ItemCountInfo item in hero.CurrentTask.Info.Rewards) {
					for (int i = 0; i < item.Count; i++) {
						reward.Creator.ItemQueue.Add(item.ChainId);
					}
				}

				mergeBoard.CreateItem(coordinates.X, coordinates.Y, reward);
				player.ClientListener.OnItemCreatedOnBoard(
					mergeBoard.Info.Type,
					reward,
					heroBuilding.X,
					heroBuilding.Y,
					coordinates.X,
					coordinates.Y,
					true
				);
				player.HandleItemDiscovery(reward);

				hero.CurrentTask.Claim(player.CurrentTime);
				player.AddActivityEventScore(
					ActivityEventType.HeroTasks,
					hero.CurrentTask.Info.HeroTaskEventScore,
					new HeroTaskResourceContext(HeroType)
				);
				player.ClientListener.OnHeroTaskModified(HeroType);
				hero.AddXp(player.GameConfig, hero.CurrentTask.Info.HeroXp, player.AddReward, player.ClientListener, player.ServerListener);
				player.EarnResources(
					CurrencyTypeId.Xp,
					hero.CurrentTask.Info.PlayerXp,
					IslandTypeId.MainIsland,
					new HeroTaskResourceContext(HeroType)
				);
				player.ProgressDailyTask(
					DailyTaskTypeId.CompleteHeroTask,
					1,
					new MergeBoardResourceContext(heroBuilding.X, heroBuilding.Y)
				);
				player.Logbook.RegisterTaskProgress(
					LogbookTaskType.HeroTasks,
					player.CurrentTime,
					player.ClientListener
				);
				player.EventStream.Event(new PlayerHeroTaskRewardsClaimed(HeroType));
			}

			return ActionResult.Success;
		}
	}
}
