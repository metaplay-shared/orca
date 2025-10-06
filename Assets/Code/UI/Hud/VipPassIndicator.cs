using Code.UI.Hud.Signals;
using Code.UI.Utils;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;

namespace Code.UI.Hud {
	public class VipPassIndicator : SignalReactor<VipPassChangedSignal> {
		[SerializeField] private GameObject VipPass;

		private void Start() {
			UpdateState();
		}

		protected override void OnSignal(VipPassChangedSignal signal) {
			UpdateState();
		}

		private void UpdateState() {
			VipPass.SetActive(MetaplayClient.PlayerModel.VipPasses.HasAnyPass);
		}
	}
}
