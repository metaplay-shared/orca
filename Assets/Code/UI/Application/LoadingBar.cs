using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Application {
	public class LoadingBar : MonoBehaviour {
		[SerializeField] private Image LoadingFiller;

		private void Start() {
			LoadingFiller.fillAmount = 0;
		}

		private void Update() {
			LoadingFiller.fillAmount = Mathf.Lerp(LoadingFiller.fillAmount, LoadingInfo.Progress, Time.deltaTime * 10);
		}
	}
}
