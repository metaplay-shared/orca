using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(CheatActionCodes.CheatAddResources)]
	[DevelopmentOnlyAction]
	public class CheatAddResources : PlayerAction {
		public CurrencyTypeId Type { get; private set; }
		public int Amount { get; private set; }

		public CheatAddResources() { }

		public CheatAddResources(CurrencyTypeId type, int amount) {
			Type = type;
			Amount = amount;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (Type == CurrencyTypeId.None) {
				return ActionResult.InvalidParam;
			}

			if (commit) {
				player.EarnResources(Type, Amount, IslandTypeId.None, ResourceModificationContext.Empty);
			}

			return ActionResult.Success;
		}
	}
}
