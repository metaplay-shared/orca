using Metaplay.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class TimerIcon : MonoBehaviour {
		[SerializeField] private Image Fill;

		public void UpdateFill(float timeLeft, float totalTime) {
			Fill.fillAmount = timeLeft / totalTime;
		}

		public void UpdateFill(MetaDuration timeLeft, MetaDuration totalTime) {
			UpdateFill(timeLeft.ToSecondsF64().Float, totalTime.ToSecondsF64().Float);
		}
	}
}
