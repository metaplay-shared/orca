using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI {
	public class ConfirmationPopup : UIRootWithResultBase<ConfirmationPopupHandle, ConfirmationPopupResult> { //Popup<ConfirmationPopup, ConfirmationPopupPayload> {
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text ContentText;

		[SerializeField] private Button YesButton;
		[SerializeField] private Button NoButton;
		[SerializeField] private Button CancelButton;
		[SerializeField] private Button CloseButton;

		protected override void Init() {
			Title.text = UIHandle.Title;
			ContentText.text = UIHandle.Text;

			CancelButton.gameObject.SetActive(
				UIHandle.Type == ConfirmationPopupHandle.ConfirmationPopupType.YesNoCancel
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		protected override async UniTask<ConfirmationPopupResult> IdleWithResult(CancellationToken ct) {
			(int _, ConfirmationPopupResponse response) = await UniTask.WhenAny(
				new [] {
					YesButton.OnClickAsync(ct).ContinueWith(() => ConfirmationPopupResponse.Yes),
					NoButton.OnClickAsync(ct).ContinueWith(() => ConfirmationPopupResponse.No),
					CancelButton.OnClickAsync(ct).ContinueWith(() => ConfirmationPopupResponse.Cancel),
					CloseButton.OnClickAsync(ct).ContinueWith(() => ConfirmationPopupResponse.Cancel),
					OnBackgroundClickAsync(ct).ContinueWith(() => ConfirmationPopupResponse.Cancel)
				}
			);
			return new ConfirmationPopupResult(response);
		}
	}

	public class ConfirmationPopupHandle : UIHandleWithResultBase<ConfirmationPopupResult> {
		public string Title { get; }
		public string Text { get; }
		public ConfirmationPopupType Type { get; }

		public enum ConfirmationPopupType {
			YesNo,
			YesNoCancel
		}

		public ConfirmationPopupHandle(string title, string text, ConfirmationPopupType type) {
			Title = title;
			Text = text;
			Type = type;
		}
	}

	public class ConfirmationPopupResult : IUIResult {
		public ConfirmationPopupResult(ConfirmationPopupResponse response) {
			Response = response;
		}
		public ConfirmationPopupResponse Response { get; }
	}

	public enum ConfirmationPopupResponse {
		Yes,
		No,
		Cancel
	}
}
