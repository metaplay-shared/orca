using Code.UI.AssetManagement;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Offers {
	public class OfferItem : MonoBehaviour {
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text Label;
		[Inject] private AddressableManager addressableManager;

		public void Setup(ChainTypeId type, int level, int count) {
			Icon.sprite = addressableManager.GetItemIcon(type, level);
			Label.text = $"x{count}";
		}

		public void Setup(CurrencyTypeId type, int count) {
			Icon.sprite = addressableManager.Get<Sprite>($"Icons/{type}.png");
			Label.text = $"x{count}";
		}
	}
}
