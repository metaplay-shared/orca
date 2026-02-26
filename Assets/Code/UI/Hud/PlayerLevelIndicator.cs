using Code.Inbox;
using Code.UI.Core;
using Code.UI.Hud.Signals;
using Code.UI.Player;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Hud {
	public class PlayerLevelIndicator : SignalReactor<PlayerLevelChangedSignal> {
		[SerializeField] private TMP_Text LevelText;
		[SerializeField] private Button LevelButton;
		[SerializeField] private GameObject Notification;

		[Inject] private IUIRootController uiRootController;
		[Inject] private IInboxController inboxController;

		protected void Awake() {
			// Notification is only shown in top hud
			if (Notification != null) {
				inboxController.HasUnreadMessages.Subscribe(active => Notification.SetActive(active)).AddTo(gameObject);
			}
		}

		protected override void OnEnable() {
			base.OnEnable();
			LevelButton.onClick.AddListener(OnLevelButtonClick);
		}

		protected override void OnDisable() {
			base.OnDisable();
			LevelButton.onClick.RemoveListener(OnLevelButtonClick);
		}

		private void OnLevelButtonClick() {
			uiRootController.ShowUI<PlayerLevelUIRoot, PlayerLevelUIHandle>(
				new PlayerLevelUIHandle(),
				this.GetCancellationTokenOnDestroy()
			);
		}

		private void Start() {
			LevelText.text = MetaplayClient.PlayerModel.Level.Level.ToString();
		}

		protected override void OnSignal(PlayerLevelChangedSignal signal) {
			LevelText.text = MetaplayClient.PlayerModel.Level.Level.ToString();
		}
	}
}
