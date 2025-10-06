using System;
using Code.UI.MergeBase.Signals;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.ItemActions {
	public class ItemActions : MonoBehaviour {
		[SerializeField] private Button SellButton;
		[SerializeField] private RectTransform ButtonsContainer;

		[Inject] private SignalBus signalBus;

		private ItemModel model;
		private IslandTypeId islandType;

		private bool CanSell => model != null && model.Info.Sellable;

		private void Start() {
			SellButton.onClick.AddListener(Sell);
		}

		private void OnEnable() {
			signalBus.Subscribe<ItemSelectedSignal>(OnItemSelected);
			Hide();
		}

		private void OnDisable() {
			signalBus.Unsubscribe<ItemSelectedSignal>(OnItemSelected);
		}

		private void OnItemSelected(ItemSelectedSignal signal) {
			if (signal.Item == null) {
				Hide();
				return;
			}
			model = ((MergeItemModelAdapter)signal.Item).Model;
			islandType = ((MergeItemModelAdapter)signal.Item).Island;

			if (!CanSell) {
				Hide();
				return;
			}

			Show();

			SellButton.gameObject.SetActive(CanSell);
		}

		private void Sell() {
			if (CanSell) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerSellMergeItem(islandType, model.X, model.Y));
			}
		}

		private void Show() {
			ButtonsContainer.gameObject.SetActive(true);
		}

		private void Hide() {
			ButtonsContainer.gameObject.SetActive(false);
		}
	}
}
