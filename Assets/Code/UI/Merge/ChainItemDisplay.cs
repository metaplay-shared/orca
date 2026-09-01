using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Merge {
	public class ChainItemDisplay : MonoBehaviour {
		[SerializeField] private Image ChainIcon;
		[SerializeField] private TMP_Text LevelText;

		public void Setup(Object info) {
			/*
			var discovered = MetaplayClient.PlayerModel.MergeBoard.ItemDiscovery.ItemDiscoveryState(info.ConfigKey);

			if (discovered == DiscoveryState.NotDiscovered) {
				ChainIcon.sprite = SpriteCatalog.Instance.Get("IconQuestionMark");
			} else {
				ChainIcon.sprite = SpriteCatalog.Instance.Get($"{info.ChainType.Value}{info.Level}");
			}

			LevelText.text = info.Level.ToString();
			*/
		}
	}
}
