using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Dialogue.Commands;
using Code.UI.Dialogue.DialogueSystem;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity;
using Metaplay.Unity.DefaultIntegration;
using Orca.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Dialogue {
	public class DialoguePopupPayload : UIHandleBase {
		public DialogueScript Script { get; }

		public DialoguePopupPayload(DialogueId dialogueId) {
			if (MetaplayClient.PlayerModel.GameConfig.Dialogues.ContainsKey(dialogueId)) {
				Script = new DialogueScript(MetaplayClient.PlayerModel.GameConfig.Dialogues[dialogueId]);
			} else {
				Debug.LogError($"Dialogue '{dialogueId}' missing");
			}
		}

		public DialoguePopupPayload(string input) {
			Script = new DialogueScript(
				new DialogueInfo(DialogueId.FromString("test"), DialogueParser.ParseDialogue(input))
			);
		}
	}

	public enum EntryType {
		None,
		Chat,
		Command
	}

	public enum SpeakerSide {
		Left,
		Right
	}

	public class DialoguePopup : UIRootBase<DialoguePopupPayload> {
		[SerializeField] private TMP_Text SpeakerText;
		[SerializeField] private TMP_Text SpeechContentText;
		[SerializeField] private GameObject TapToContinue;
		[SerializeField] private Button TapButton;

		[SerializeField] private GameObject SpeakerLeft;
		[SerializeField] private Image SpeakerLeftImage;
		[SerializeField] private GameObject SpeakerRight;
		[SerializeField] private Image SpeakerRightImage;

		[SerializeField] private TextFader TextFader;

		[Inject] private AddressableManager addressableManager;

		private readonly DialogueCommandRunner commandRunner = new();
		private bool skipped = false;

		public void SetSpeaker(string speakerId, string expression, SpeakerSide side) {
			SetupSpeakerSide(SpeakerLeft, SpeakerLeftImage, side == SpeakerSide.Left);
			SetupSpeakerSide(SpeakerRight, SpeakerRightImage, side == SpeakerSide.Right);

			void SetupSpeakerSide(GameObject speakerGo, Image speakerImage, bool visible) {
				speakerGo.SetActive(visible);
				speakerImage.sprite = visible
					? addressableManager.Get<Sprite>($"Heroes/{speakerId}.png")
					: null;
			}
		}

		protected override void Init() {
			SpeakerLeft.gameObject.SetActive(false);
			SpeakerRight.gameObject.SetActive(false);

			commandRunner.RegisterCommand(new SpeakerCommand(this));

			SetInitialSpeakerContent();
		}

		private void SetInitialSpeakerContent() {
			ChatDialogueEntryInfo chatDialogueEntryInfo = UIHandle.Script.GetFirstChatDialogue();
			if (chatDialogueEntryInfo != null) {
				SetChatContent(chatDialogueEntryInfo);
				SpeechContentText.text = string.Empty;
				TapToContinue.SetActive(false);
			}

			CommandDialogueEntryInfo commandDialogueEntryInfo = UIHandle.Script.GetFirstSpeaker();
			if (commandDialogueEntryInfo != null) {
				commandRunner.RunCommand(commandDialogueEntryInfo);
			}
		}

		protected override Option<Tween> CreateShowAnimation() {
			Sequence sequence = DOTween.Sequence();
			Option<Tween> uiRootTween = base.CreateShowAnimation();
			foreach (Tween tween in uiRootTween) {
				sequence.Join(tween);
				sequence.Join(SpeakerLeftImage.DOFade(1f, tween.Duration()));
				sequence.Join(SpeakerRightImage.DOFade(1f, tween.Duration()));
			}

			return sequence;
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await Loop();
		}

		protected override void HandleAndroidBackButtonPressed() { }

		private async UniTask Loop() {
			while (Next(out var type) && !skipped) {
				TapToContinue.SetActive(false);

				if (type != EntryType.Chat) {
					continue;
				}

				// Animate text;
				await AnimateText();
				if (skipped) {
					continue;
				}

				TapToContinue.SetActive(true);

				// Wait for input
				await WaitForInput();
			}
		}

		private async UniTask AnimateText() {
			CancellationTokenSource cancellationTokenSource =
				CancellationTokenSource.CreateLinkedTokenSource(gameObject.GetCancellationTokenOnDestroy());
			TapButton.onClick.AddListener(Skip);

			await TextFader.FadeIn(cancellationTokenSource.Token);

			TapButton.onClick.RemoveListener(Skip);

			void Skip() {
				cancellationTokenSource.Cancel();
			}
		}

		private async UniTask WaitForInput() {
			UniTaskCompletionSource tcs = new();

			TapButton.onClick.AddListener(Continue);

			await tcs.Task;

			TapButton.onClick.RemoveAllListeners();

			void Continue() {
				tcs.TrySetResult();
			}
		}

		private bool Next(out EntryType type) {
			type = EntryType.None;

			if (UIHandle.Script == null ||
				!UIHandle.Script.TryGetNext(out var entry)) {
				return false;
			}

			switch (entry) {
				case ChatDialogueEntryInfo chatEntry: {
					SetChatContent(chatEntry);
					type = EntryType.Chat;
					break;
				}
				case CommandDialogueEntryInfo commandEntry:
					commandRunner.RunCommand(commandEntry);
					type = EntryType.Command;
					break;
			}

			return true;
		}

		private void SetChatContent(ChatDialogueEntryInfo chatEntry) {
			SpeakerText.text = Localizer.Localize($"DialogueSpeaker.{chatEntry.Speaker}");
			SpeechContentText.text = chatEntry.Text;
			// Don't localize for now
			// SpeechContentText.text = Localizer.Localize(chatEntry.GetLocalizationKey(UIHandle.Script.Dialogue.Id));
			// if (SpeechContentText.text == Localizer.NO_KEY) {
			// 	SpeechContentText.text = "##" + chatEntry.Text;
			// }

			// TODO: [henri] move to client listener on the action
#if UNITY_WEBGL && !UNITY_EDITOR
			GameWebGLApiBridge.UpdateChatDialogue(SpeakerText.text, SpeechContentText.text);
			GameWebGLApiBridge.UpdateInfoUrl(chatEntry.InfoUrl);
#endif
			MetaplaySDK.RunOnMainThreadAsync(() => MetaplayClient.PlayerContext.ExecuteAction(new PlayerSetLatestInfoUrl(chatEntry.InfoUrl)));
		}

		public void SkipAll() {
			skipped = true;
			TapButton.onClick.Invoke();
		}
	}
}
