using Code.UI.Hud.Signals;
using Code.UI.Utils;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Hud {
	public class PlayerXpIndicator : SignalReactor<PlayerXpChangedSignal> {
		[SerializeField] private Image RadialXpImage;

		protected override void OnSignal(PlayerXpChangedSignal signal) {
			UpdateXp();
		}

		private void Start() {
			UpdateXp();
		}

		private void UpdateXp() {
			var levelInfo = MetaplayClient.PlayerModel.GameConfig.PlayerLevels[MetaplayClient.PlayerModel.Level.Level];
			RadialXpImage.fillAmount = (float)MetaplayClient.PlayerModel.Level.CurrentXp / levelInfo.XpToNextLevel;
		}
	}
}
