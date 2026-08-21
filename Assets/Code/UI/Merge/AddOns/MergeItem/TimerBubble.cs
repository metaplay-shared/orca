using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class TimerBubble : MonoBehaviour {
		[SerializeField] private RectTransform Bubble;
		[SerializeField] private Button SkipButton;
		[SerializeField] private CurrencyLabel Cost;
		[SerializeField] private TMP_Text TimerText;

		[Inject] private DiContainer container;

		public void Show(RectTransform overlayLayer, Transform parent) {
			Bubble.SetParent(parent, false);
			Bubble.anchoredPosition = Vector3.zero;
			Bubble.SetParent(overlayLayer, true);
			Bubble.localScale = Vector3.one;
			container.Inject(Cost);

			Bubble.gameObject.SetActive(true);
		}

		public void Hide() {
			if (Bubble != null) {
				Bubble.gameObject.SetActive(false);
			}
		}

		public void AddClickListener(UnityAction listener) {
			SkipButton.onClick.AddListener(listener);
		}

		public void SetCostAndTimer(CurrencyTypeId type, int cost, string timeStr) {
			Cost.Set(type, cost);
			TimerText.text = timeStr;
		}
	}
}
