using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge {
	public class ItemInfoView : MonoBehaviour {
		[Header("Item details")]
		[SerializeField] private Image Icon;
		[SerializeField] private Button ItemInfoButton;
		[SerializeField] private TMP_Text ItemName;
		//[SerializeField] private UpgradeTimerLabel OpenTimer;

		[Header("Buttons")]
		[SerializeField] private Button SellButton;
		[SerializeField] private TMP_Text SellButtonText;
		[SerializeField] private Button OpenButton;

		[Inject] private SignalBus signalBus;

		//private ItemModel selectedItem;

		private void Awake() {
			//signalBus.Subscribe<ItemSelectedSignal>(OnItemSelected);
			//signalBus.Subscribe<ItemStateChangedSignal>(OnItemStateChanged);
			gameObject.SetActive(false);
		}

		/*
		private void UpdateState(ItemModel model) {
			SellButton.gameObject.SetActive(selectedItem.Info.Sellable);

			Icon.sprite = SpriteCatalog.Instance.Get($"{model.Info.ChainType.Value}{model.Info.Level}");
			ItemName.text = $"{model.Info.ChainType.Localize()} {model.Info.Level}";

			SellButton.onClick.RemoveAllListeners();
			SellButton.onClick.AddListener(Sell);

			OpenButton.onClick.RemoveAllListeners();
			OpenButton.onClick.AddListener(Open);

			SellButton.gameObject.SetActive(
				!model.Locked &&
				!model.Bubble &&
				model.Info.Sellable &&
				model.State == ItemState.Open
			);

			if (model.State == ItemState.Opening) {
				OpenTimer.gameObject.SetActive(true);
				OpenTimer.End = model.OpenAt;
			} else {
				OpenTimer.gameObject.SetActive(false);
			}

			OpenButton.gameObject.SetActive(model.Bubble || model.State == ItemState.Opening);

			ItemInfoButton.onClick.RemoveAllListeners();
			ItemInfoButton.onClick.AddListener(
				() => { ChainInfoPopup.Show(new ChainInfoPopupPayload(model.Info.ChainType)); }
			);
		}*/

		/*
		private void Sell() {
			var coordinates = MetaplayClient.PlayerModel.MergeBoard.FindCoordinates(selectedItem);
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerSellMergeItem(coordinates.X, coordinates.Y));
		}
		*/

		/*
		private void Open() {
			var coordinates = MetaplayClient.PlayerModel.MergeBoard.FindCoordinates(selectedItem);

			if (selectedItem.Bubble) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerOpenBubble(coordinates.X, coordinates.Y));
			} else {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerSkipMergeItemOpen(coordinates.X, coordinates.Y));
			}
		}
		*/
	}
}
