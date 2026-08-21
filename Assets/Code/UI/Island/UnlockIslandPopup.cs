using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System.Threading;
using Code.UI.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Island {
	public class UnlockIslandPopupHandle : UIHandleBase {
		public IslandTypeId IslandTypeId { get; }

		public UnlockIslandPopupHandle(IslandTypeId islandTypeId) {
			IslandTypeId = islandTypeId;
		}
	}

	public class UnlockIslandPopup : UIRootBase<UnlockIslandPopupHandle> {
		[SerializeField] private Button UnlockIslandButton;
		[SerializeField] private Button CloseButton;
		[SerializeField] private TMP_Text TitleText;
		[SerializeField] private Image IslandImage;

		protected override void Init() {
			TitleText.text = Localizer.Localize($"Island.{UIHandle.IslandTypeId}");
			IslandImage.SetSpriteFromAddressableAssetsAsync(
				$"Islands/{UIHandle.IslandTypeId.Value}.png",
				gameObject.GetCancellationTokenOnDestroy()
			).Forget();
		}

		protected override async UniTask Idle(CancellationToken ct) {
			(_, bool unlockRequested) = await UniTask.WhenAny(
				new[] {
					UnlockIslandButton.OnClickAsync(ct).ContinueWith(() => true),
					OnDismissAsync(ct).ContinueWith(() => false)
				}
			);

			if (unlockRequested) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerUnlockIsland(UIHandle.IslandTypeId));
			}
		}

		private UniTask OnDismissAsync(CancellationToken ct) {
			return UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}
}
