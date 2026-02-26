using System.Collections.Generic;
using Game.Logic;
using Metaplay.Core.Config;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using Metaplay.Core.Rewards;

namespace Game.Logic.LiveOpsEvents {
	[LiveOpsEvent(1, "Currency Multiplier")]
	public class CurrencyMultiplierEvent : LiveOpsEventContent
	{
		[MetaMember(1)]
		public float Multiplier { get; set; }
		
		[MetaMember(2)]
		public CurrencyTypeId Type { get; set; }
	}

	[MetaSerializableDerived(1)]
	public class CurrencyMultiplierState : PlayerLiveOpsEventModel<CurrencyMultiplierEvent> {
		
	}

	[MetaSerializable]
	public class RewardTier {
		[MetaMember(1)]
		public int Threshold { get; set; }
		
		[MetaMember(2)]
		public List<PlayerReward> Rewards { get; set; }

		public RewardTier()
		{
		}
	}
	
	[LiveOpsEvent(2, "Merge Event")]
	public class MergeEvent : LiveOpsEventContent
	{
		[MetaMember(1)]
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