using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Metaplay.Unity;
using UnityEngine;
using Zenject;

namespace Code.UI.Cheats {
	public class ShowCheatsButton : ButtonHelper {
		[Inject] private IUIRootController uiRootController;
		private bool isTriggering;
		private float startTime;
		private bool hasOpenedCheats;

		private void Start() {
			if (MetaplayClient.PlayerModel.IsDeveloper ||
				MetaplaySDK.Connection.ServerOptions.EnableDevelopmentFeatures) {
				#if !UNITY_EDITOR
				foreach (UnityEngine.UI.Graphic graphic in GetComponentsInChildren<UnityEngine.UI.Graphic>()) {
					graphic.enabled = false;
				}
				#endif
			} else {
				Destroy(gameObject);
			}
		}

		protected override void OnClick() {
			OpenCheatsMenu();
		}

		private void OpenCheatsMenu() {
			uiRootController.ShowUI<CheatsUIRoot, CheatsUIHandle>(
				new CheatsUIHandle(),
				gameObject.GetCancellationTokenOnDestroy()
			);
		}

		private void Update() {
			if (IsTriggeringCheats()) {
				if (!isTriggering) {
					isTriggering = true;
					startTime = Time.time;
				}

				if (!hasOpenedCheats && Time.time >= startTime + 0.5f) {
					OpenCheatsMenu();
					hasOpenedCheats = true;
				}
			} else {
				isTriggering = false;
				hasOpenedCheats = false;
			}
		}

		private bool IsTriggeringCheats() {
			#if UNITY_EDITOR
			return Input.GetKey(KeyCode.Z);
			#else
			return Input.touchCount == 4;
			#endif
		}
	}
}
