using Code.UI.Utils;
using Game.Logic;
using TMPro;
using UnityEngine;

namespace Code.UI.Market {
	public class MarketCategory : MonoBehaviour {
		[SerializeField] private TMP_Text TitleText;
		[SerializeField] public Transform Content;

		public void Setup(ShopCategoryId category) {
			TitleText.text = category.Localize();
		}
	}
}
