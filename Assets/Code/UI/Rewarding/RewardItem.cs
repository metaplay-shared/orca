using System;
using Code.UI.AssetManagement;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Rewarding {
	public class RewardItem : MonoBehaviour {
		[SerializeField] private Image ItemIcon;
		[SerializeField] private TMP_Text CountLabel;
		[Inject] private AddressableManager addressableManager;

		public void Setup(CurrencyTypeId type, int count) {
			SetCount(count);
			try {
				ItemIcon.sprite = addressableManager.Get<Sprite>($"Icons/{type}.png");
			} catch (Exception e) {
				Debug.LogException(e);
			}
		}

		public void Setup(ChainTypeId type, int level, int count) {
			SetCount(count);
			try {
				ItemIcon.sprite = addressableManager.GetItemIcon(type, level);
			} catch (Exception e) {
				Debug.LogException(e);
			}
		}

		private void SetCount(int count) {
			if (count > 1) {
				CountLabel.text = count + "x";
			} else {
				CountLabel.gameObject.SetActive(false);
			}
		}
	}
}
