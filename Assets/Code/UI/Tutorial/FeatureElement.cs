using System;
using Code.UI.Application;
using Code.UI.Application.Signals;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Tutorial {
	// WARNING!
	// Assign value to each entry and NEVER change them. UNITY editor uses the value, not the name.
	// Failing to comply will break something.
	public enum FeatureType {
		Map = 0,
		Shop = 1,
		Inventory = 2,
		Collection = 3,
		Logbook = 4,
		ItemRemoval = 5,
		DailyTaskEvents = 6
	}

	public class FeatureElement : MonoBehaviour {
		[SerializeField] private FeatureType FeatureType;
		private FeatureTypeId feature;
		[Inject] private SignalBus signalBus;

		private void Awake() {
			feature = FeatureTypeId.FromString(FeatureType.ToString());

			if (!MetaplayClient.PlayerModel.PrivateProfile.FeaturesEnabled.Contains(feature)) {
				signalBus.Subscribe<FeatureUnlockedSignal>(UpdateState);
				gameObject.SetActive(false);
			}
		}

		private void UpdateState(FeatureUnlockedSignal signal) {
			if (signal.Feature == feature && MetaplayClient.PlayerModel.PrivateProfile.FeaturesEnabled.Contains(feature)) {
				gameObject.SetActive(true);
				signalBus.Unsubscribe<FeatureUnlockedSignal>(UpdateState);
			}
		}
	}
}
