using Code.Privacy;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Settings {
	public class OpenTermsOfServiceButton : MonoBehaviour {
		[SerializeField] private Button Button;

		private void Awake() {
			Button.OnClickAsObservable().Subscribe(_ => HandleButtonClicked()).AddTo(gameObject);
		}

		private void HandleButtonClicked() {
			// UnityEngine.Application.OpenURL(PrivacyConstants.TERMS_OF_SERVICE_URL);
		}
	}
}
