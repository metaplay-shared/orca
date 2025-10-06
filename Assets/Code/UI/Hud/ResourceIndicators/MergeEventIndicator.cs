using System.Linq;
using Code.UI.Application.Signals;
using Code.UI.HudBase;
using Game.Logic;
using Game.Logic.LiveOpsEvents;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;

namespace Code.UI.Hud.ResourceIndicators {
	public class MergeEventIndicator : ResourceIndicatorBase {
		protected override int ResourceAmount => (MetaplayClient.PlayerModel.LiveOpsEvents.EventModels.FirstOrDefault(x=>x.Value is MergeEventState).Value as MergeEventState)?.MergeScore ?? 0;
		protected override CurrencyTypeId Type => CurrencyTypeId.MergeEvent;

		protected override void Start()
		{
			base.Start();
			signalBus.Subscribe<FeatureUnlockedSignal>(OnFeatureUnlocked);
			CheckEnabled();
		}

		private void OnFeatureUnlocked(FeatureUnlockedSignal feature)
		{
			CheckEnabled();
		}

		void CheckEnabled()
		{
			if (MetaplayClient.PlayerModel.LiveOpsEvents.EventModels.Values.Where(x=>x.Phase.IsActivePhase()).Select(x=>x.Content).OfType<MergeEvent>().Any())
			{
				if (MetaplayClient.PlayerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.Shop))
				{
					ResourceAmountText.text = ResourceAmount.ToString();
					gameObject.SetActive(true);
					return;
				}
			}
			
			gameObject.SetActive(false);
		}

		void OnDestroy()
		{
			signalBus.Unsubscribe<FeatureUnlockedSignal>(OnFeatureUnlocked);
		}

		public override void OnClick() {
		}
	}
}
