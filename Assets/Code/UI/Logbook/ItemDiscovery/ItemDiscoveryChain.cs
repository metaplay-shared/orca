using Code.UI.Utils;
using Game.Logic;
using TMPro;
using UnityEngine;

namespace Code.UI.ItemDiscovery {
	public class ItemDiscoveryChain : MonoBehaviour {
		[SerializeField] private TMP_Text TitleText;
		[SerializeField] public RectTransform Content;

		public void Setup(ChainTypeId chain) {
			TitleText.text = chain.Localize();
		}
	}
}
