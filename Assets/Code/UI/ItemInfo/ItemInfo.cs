using Code.UI.Application;
using Code.UI.MergeBase.Signals;
using Code.UI.Utils;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.ItemInfo {
	public class ItemInfo : MonoBehaviour {
		[SerializeField] private GameObject Background;
		[SerializeField] private TMP_Text ItemNameText;

		[Inject] private SignalBus signalBus;
		[Inject] private ApplicationInfo applicationInfo;

		private void OnEnable() {
			signalBus.Subscribe<ItemSelectedSignal>(OnItemSelected);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ItemSelectedSignal>(OnItemSelected);
		}

		private void OnItemSelected(ItemSelectedSignal signal) {
			if (signal.Item == null) {
				Background.SetActive(false);
			} else {
				Background.SetActive(true);
				string levelText = Localizer.Localize("Info.Level", signal.Item.Level);
				ItemNameText.text = $"{Localizer.Localize($"Chain.{signal.Item.Type}.{signal.Item.Level}")} " + levelText;
			}
		}
	}
}
