using System.Collections.Generic;
using Game.Logic;
using Metaplay.Core.Config;
using Metaplay.Core.Forms;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using Metaplay.Core.Rewards;

namespace Game.Logic.LiveOpsEvents {
	[LiveOpsEvent(1, "Currency Multiplier")]
	public class CurrencyMultiplierEvent : LiveOpsEventContent
	{
		[MetaMember(1),
		MetaFormDisplayProps(displayName: "Currency Multiplier",
			DisplayHint = "The multiplier value to apply to the specified rewarded currency during this event.",
			DisplayPlaceholder = "Enter multiplier value")]
		public float Multiplier { get; set; }
		
		[MetaMember(2),
		MetaFormDisplayProps(displayName: "Currency Type",
			DisplayHint = "The type of currency to which the reward multiplier will be applied.",
			DisplayPlaceholder = "Select currency type")]
		public CurrencyTypeId Type { get; set; }
	}

	[MetaSerializableDerived(1)]
	public class CurrencyMultiplierState : PlayerLiveOpsEventModel<CurrencyMultiplierEvent> {
		
	}

	[MetaSerializable]
	public class RewardTier {
		[MetaMember(1),
		MetaFormDisplayProps(displayName: "Threshold",
			DisplayHint = "The score threshold for this reward tier.",
			DisplayPlaceholder = "Enter score threshold")]
		public int Threshold { get; set; }
		
		[MetaMember(2),
		MetaFormDisplayProps(displayName: "Rewards",
			DisplayHint = "List of rewards granted when the score threshold is reached.",
			DisplayPlaceholder = "Define rewards for this tier")]
		public List<PlayerReward> Rewards { get; set; }

		public RewardTier()
		{
		}
	}
	
	[LiveOpsEvent(2, "Merge Event")]
	public class MergeEvent : LiveOpsEventContent
	{
		[MetaMember(1),
		MetaFormDisplayProps(displayName: "Reward Tiers",
			DisplayHint = "Define the reward tiers for this merge event. Each tier has a score threshold and associated rewards.",
			DisplayPlaceholder = "Define reward tiers")]
		public List<RewardTier> RewardTiers { get; set; }
	}

	[MetaSerializableDerived(2)]
	public class MergeEventState : PlayerLiveOpsEventModel<MergeEvent> {
		[MetaMember(1)]
		public int MergeScore { get; set; }

		public override void OnPhaseChanged(
			IPlayerModelBase player,
			LiveOpsEventPhase oldPhase,
			LiveOpsEventPhase[] fastForwardedPhases,
			LiveOpsEventPhase newPhase
		) {
			if (newPhase.IsEndedPhase()) {
				foreach (RewardTier rewardTier in Content.RewardTiers) {
					if (MergeScore >= rewardTier.Threshold) {
						foreach (var reward in rewardTier.Rewards) {
							reward.InvokeConsume(player, null);
						}
					}
				}
			}
		}

		public void AddScore(int score) {
			if (Phase.IsActivePhase())
				MergeScore += score;
		}
	}
}