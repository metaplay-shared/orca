using System.Threading;
using Code.UI.Builders;
using Code.UI.Core;
using Code.UI.Hud.Signals;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Hud.ResourceIndicators {
	public class BuilderIndicator : MonoBehaviour {
		[SerializeField] private TMP_Text Text;

		[Inject] private SignalBus signalBus;
		[Inject] private IUIRootController uiRootController;

		public void OnClick() {
			if (MetaplayClient.PlayerModel.PrivateProfile.FeaturesEnabled.Contains(FeatureTypeId.HudButtonBuilders)) {
				uiRootController.ShowUI<BuilderPopup, BuilderPopupPayload>(
					new BuilderPopupPayload(),
					CancellationToken.None
				);
			}
		}

		private void Start() {
			OnBuildersChanged();
		}

		private void OnEnable() {
			signalBus.Subscribe<BuildersChangedSignal>(OnBuildersChanged);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<BuildersChangedSignal>(OnBuildersChanged);
		}

		private void OnBuildersChanged() {
			Text.text = MetaplayClient.PlayerModel.Builders.Free + "/" + MetaplayClient.PlayerModel.Builders.Total;
		}
	}
}
