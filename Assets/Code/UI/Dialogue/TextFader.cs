using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Code.UI.Dialogue {
	public class TextFader : MonoBehaviour {
		private const string DELIMITERS = ".,!?";

		[SerializeField] private int Delay = 10;
		[SerializeField] private int DelimiterDelay = 20;

		private TMP_Text textComponent;
		private CancellationTokenSource cancellationTokenSource;
		private string currentRenderingText;

		public bool IsRunning => !cancellationTokenSource.IsCancellationRequested;

		public void Cancel() {
			cancellationTokenSource.Cancel();
			textComponent.maxVisibleCharacters = textComponent.text.Length;
			textComponent.ForceMeshUpdate();
		}

		protected void Awake() {
			textComponent = GetComponent<TMP_Text>();
		}

		protected void OnEnable() {
			//TMPro_EventManager.TEXT_CHANGED_EVENT.Add(TextChanged);
		}

		protected void OnDisable() {
			//TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(TextChanged);
		}

		private void TextChanged(Object obj) {
			if (obj != null &&
				obj == textComponent) {
				TextChangedAsync(this.GetCancellationTokenOnDestroy()).Forget();
			}
		}

		private async UniTask TextChangedAsync(CancellationToken ct) {
			try {
				using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct)) {
					cancellationTokenSource.Cancel();
					cancellationTokenSource = cts;
					await FadeIn(cts.Token);
				}
			} finally {
				cancellationTokenSource = default;
			}
		}

		public async UniTask FadeIn(CancellationToken ct) {
			currentRenderingText = textComponent.text;
			textComponent.maxVisibleCharacters = 0;
			textComponent.ForceMeshUpdate();

			var textInfo = textComponent.textInfo;
			var charactersShown = 0;
			var characterCount = textInfo.characterCount;

			while (charactersShown < characterCount) {
				//ct.ThrowIfCancellationRequested();

				// When the current shown character index is n, the amount of shown characters is n+1.
				textComponent.maxVisibleCharacters = charactersShown + 1;
				textComponent.ForceMeshUpdate();

				var characterInfo = textInfo.characterInfo[charactersShown];
				var currentCharacterIsDelimiter = DELIMITERS.Contains(characterInfo.character);

				if (!ct.IsCancellationRequested) {
					await UniTask.Delay(
						currentCharacterIsDelimiter
							? DelimiterDelay
							: Delay,
						cancellationToken: CancellationToken.None
					);
				}

				charactersShown++;
			}
		}
	}
}
