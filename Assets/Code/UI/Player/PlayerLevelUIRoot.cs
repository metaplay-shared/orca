using Code.Inbox;
using System.Threading;
using Code.UI.Core;
using Code.UI.Deletion;
using Code.UI.Inbox;
using Code.UI.Settings;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Toggle = Code.UI.Settings.Toggle;

namespace Code.UI.Player {
	public class PlayerLevelUIHandle : UIHandleBase { }

	public class PlayerLevelUIRoot : UIRootBase<PlayerLevelUIHandle> {
		[SerializeField] private Button CloseButton;

		[SerializeField] private Button InboxButton;
		[SerializeField] private Button DeleteAccountButton;
		[SerializeField] private GameObject InboxNotification;

		[SerializeField] private TMP_Text PlayerLevelText;
		[SerializeField] private TMP_Text XpText;
		[SerializeField] private TMP_Text PlayerIdText;

		[SerializeField] private Image XpProgressFill;

		[SerializeField] private Toggle SoundToggle;
		[SerializeField] private Toggle MusicToggle;

		[Inject] private IUIRootController uiRootController;
		[Inject] private IInboxController inboxController;
		[Inject] private IDeletionController deletionController;

		protected override void Init() {
			PlayerModel playerModel = MetaplayClient.PlayerModel;
			int currentXp = playerModel.Level.CurrentXp;
			int requiredNextLevelXp = playerModel.Level.TotalXp;

			if (MetaplayClient.PlayerModel.GameConfig.PlayerLevels.TryGetValue(
					MetaplayClient.PlayerModel.Level.Level,
					out PlayerLevelInfo levelInfo
				)) {
				requiredNextLevelXp = levelInfo.XpToNextLevel;
			}

			PlayerIdText.text = playerModel.PlayerId.ToString();
			PlayerLevelText.text = playerModel.PlayerLevel.ToString();
			XpText.text = Localizer.Localize(
				"Info.CurrentToTargetXp",
				currentXp,
				requiredNextLevelXp
			);

			XpProgressFill.DOFillAmount((float)currentXp / requiredNextLevelXp, 0.5f);

			SoundToggle.Setup(
				Localizer.Localize("Settings.Sound"),
				MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.SoundEnabled,
				val => MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerSetSoundSettings(
						val,
						MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.MusicEnabled
					)
				)
			);

			MusicToggle.Setup(
				Localizer.Localize("Settings.Music"),
				MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.MusicEnabled,
				val => MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerSetSoundSettings(
						MetaplayClient.PlayerModel.PrivateProfile.SoundSettings.SoundEnabled,
						val
					)
				)
			);

			inboxController.HasUnreadMessages.Subscribe(active => InboxNotification.SetActive(active)).AddTo(gameObject);
		}

		protected override async UniTask Idle(CancellationToken ct) {
			InboxButton.OnClickAsAsyncEnumerable(ct).ForEachAwaitAsync(arg => OpenInbox(arg, ct), ct).Forget();
			DeleteAccountButton.OnClickAsAsyncEnumerable(ct).ForEachAwaitAsync(arg => TryConfirmDeletion(arg, ct), ct).Forget();

			await UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		private UniTask OpenInbox(AsyncUnit arg, CancellationToken ct) {
			return uiRootController.ShowUI<InboxPopup, InboxPopupHandle>(new InboxPopupHandle(), ct).OnComplete;
		}

		private UniTask TryConfirmDeletion(AsyncUnit arg, CancellationToken ct) {
			return deletionController.TryConfirmDeletion(ct);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}
}
