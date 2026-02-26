using Metaplay.Core;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class TriggerModel {
		[MetaMember(1)]
		public MetaDictionary<TriggerId, MetaTime> Executed = new MetaDictionary<TriggerId, MetaTime>();

		public void ExecuteTrigger(PlayerModel player, TriggerId triggerId) {
			if (Executed.ContainsKey(triggerId)) {
				return;
			}

			if (!player.GameConfig.Triggers.ContainsKey(triggerId)) {
				return;
			}

			TriggerInfo trigger = player.GameConfig.Triggers[triggerId];
			if (trigger.Dialogue != DialogueId.None) {
				player.ClientListener.OnDialogueStarted(trigger.Dialogue);
			}

			if (trigger.Feature != FeatureTypeId.None) {
				if (!player.PrivateProfile.FeaturesEnabled.Contains(trigger.Feature)) {
					player.UnlockFeature(trigger.Feature);
				}
			}

			if (!string.IsNullOrEmpty(trigger.HighlightElement)) {
				player.ClientListener.OnHighlightElement(trigger.HighlightElement);
			}

			if (trigger.HighlightItem.Type != ChainTypeId.None) {
				player.ClientListener.OnHighlightItem(trigger.HighlightItem.Type, trigger.HighlightItem.Level);
			}

			if (trigger.PointItem.Type != ChainTypeId.None) {
				player.ClientListener.OnPointItem(trigger.PointItem.Type, trigger.PointItem.Level);
			}

			if (trigger.MergeHint.Type != ChainTypeId.None) {
				player.ClientListener.OnMergeHint(
					trigger.MergeHint.Type,
					trigger.MergeHint.Level,
					trigger.MergeHint.Type,
					trigger.MergeHint.Level
				);
			}

			if (trigger.GoToIsland != IslandTypeId.None) {
				player.ClientListener.OnGoToIsland(trigger.GoToIsland);
			}

			if (trigger.HighlightIsland != IslandTypeId.None) {
				player.ClientListener.OnHighlightIsland(trigger.HighlightIsland);
			}

			if (trigger.PointIsland != IslandTypeId.None) {
				player.ClientListener.OnPointIsland(trigger.PointIsland);
			}

			if (trigger.Offer != InAppProductId.FromString("None")) {
				player.ClientListener.OnOpenOffer(trigger.Offer);
			}

			if (trigger.UnlockResourceItem != ChainTypeId.None) {
				player.Inventory.UnlockResourceItem(trigger.UnlockResourceItem);
			}

			if (trigger.UnlockResourceCreator != ChainTypeId.None) {
				player.Inventory.UnlockResourceCreator(trigger.UnlockResourceCreator);
			}

			player.EventStream.Event(new PlayerTriggerExecuted(triggerId));

			Executed[triggerId] = player.CurrentTime;
		}
	}
}
