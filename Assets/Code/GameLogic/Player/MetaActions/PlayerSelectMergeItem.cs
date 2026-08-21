using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSelectMergeItem)]
	public class PlayerSelectMergeItem : PlayerAction {
		public ChainTypeId Type { get; private set; }
		public int Level { get; private set; }

		public PlayerSelectMergeItem() { }

		public PlayerSelectMergeItem(ChainTypeId type, int level) {
			Type = type;
			Level = level;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			ChainInfo info = player.GameConfig.Chains[new LevelId<ChainTypeId>(Type, Level)];
			if (player.Merge.ItemDiscovery.GetState(info.ConfigKey) == DiscoveryState.NotDiscovered) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				foreach (TriggerId trigger in info.SelectedTriggers) {
					player.ExecuteTrigger(trigger);
				}
			}

			return ActionResult.Success;
		}
	}
}
