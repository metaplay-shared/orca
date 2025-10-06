using System.Threading;
using Code.UI.Application;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.InfoMessage.Signals;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class MineAddOn : ItemAddOn {
		[SerializeField] private RectTransform ItemBubble;
		[SerializeField] private Button ItemButton;
		[SerializeField] private Image ItemIcon;

		[SerializeField] private RectTransform StartMiningBubble;
		[SerializeField] private Transform MineCostContainer;
		[SerializeField] private CurrencyLabel MineCostLabelTemplate;
		[SerializeField] private Material MineCostLabelMaterial;
		[SerializeField] private Color MineCostLabelVertexColor = Color.black;
		[SerializeField] private float CurrencyLabelFontMaxSize = 45f;
		[SerializeField] private TMP_Text MineButtonLabel;
		[SerializeField] private Button StartMiningButton;

		[SerializeField] private RectTransform RepairMineBubble;
		[SerializeField] private TMP_Text RepairMineTimeText;
		[SerializeField] private Button RepairMineButton;

		[Inject] private AddressableManager addressableManager;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IUIRootController uiRootController;
		[Inject] private DiContainer container;
		private bool showItemBubble;

		protected override void Setup() {
			if (ItemModel.Mine == null) {
				return;
			}

			MineButtonLabel.text = Localizer.Localize(ItemModel.Mine.Info.Chest ? "Mine.OpenChest" : "Mine.Mine");
		}

		public override void OnStateChanged() {
			if (ItemModel.Mine == null) {
				return;
			}
			UpdateCost();
			HideBubble(StartMiningBubble);
			HideBubble(RepairMineBubble);
			if (ItemModel.Mine.Queue.Count > 0 && ItemModel.Mine.State == MineState.ItemsComplete) {
				LevelId<ChainTypeId> typeId = new LevelId<ChainTypeId>(
					MetaplayClient.PlayerModel.MapRewardToRealTypeUI(
						applicationInfo.ActiveIsland.Value,
						ItemModel.Mine.Queue[0].Type
					),
					ItemModel.Mine.Queue[0].Level
				);
				if (typeId.Type == ChainTypeId.None) {
					HideBubble(ItemBubble);
					showItemBubble = false;
				} else {
					ChainInfo itemInfo = MetaplayClient.PlayerModel.GameConfig.Chains[typeId];
					ItemIcon.sprite = addressableManager.GetItemIcon(itemInfo);

					ShowBubble(ItemBubble);
					showItemBubble = true;
				}
			} else {
				HideBubble(ItemBubble);
				showItemBubble = false;
			}
		}

		private void UpdateCost() {
			foreach (Transform child in MineCostContainer) {
				Destroy(child.gameObject);
			}

			InstantiateCurrencyLabel(CurrencyTypeId.Energy, ItemModel.Mine.EnergyUsage);
			if (ItemModel.Mine.Info.RequiresBuilder) {
				InstantiateCurrencyLabel(CurrencyTypeId.Builders, 1);
			}

			void InstantiateCurrencyLabel(CurrencyTypeId typeId, int amount) {
				CurrencyLabel currencyLabel =
					container.InstantiatePrefabForComponent<CurrencyLabel>(MineCostLabelTemplate, MineCostContainer);
				currencyLabel.Set(typeId, amount);
				currencyLabel.TextComponent.enableAutoSizing = true;
				currencyLabel.TextComponent.fontSizeMax = CurrencyLabelFontMaxSize;
				currencyLabel.TextComponent.enableWordWrapping = false;
				currencyLabel.TextComponent.material = MineCostLabelMaterial;
				currencyLabel.TextComponent.color = MineCostLabelVertexColor;
			}
		}

		public override void OnSelected() {
			if (ItemModel.Mine == null || ItemModel.State != ItemState.Free) {
				return;
			}
			if (ItemModel.Mine.State == MineState.Idle) {
				ShowBubble(StartMiningBubble);
			} else if (ItemModel.Mine.State == MineState.NeedsRepair) {
				RepairMineTimeText.text = ItemModel.Mine.RepairTime.ToSimplifiedString();
				ShowBubble(RepairMineBubble);
			}
		}

		public override void OnDeselected() {
			HideBubble(StartMiningBubble);
			HideBubble(RepairMineBubble);
		}

		public override void OnBeginDrag() {
			HideBubble(StartMiningBubble);
			HideBubble(RepairMineBubble);
			HideBubble(ItemBubble);
		}

		public override void OnEndDrag() {
			if (showItemBubble) {
				ShowBubble(ItemBubble);
			}
		}

		public override void OnBeginMove() {
			HideBubble(ItemBubble);
		}

		public override void OnEndMove() {
			if (showItemBubble) {
				ShowBubble(ItemBubble);
			}
		}

		public override void OnDestroySelf() {
			HideBubble(StartMiningBubble);
			HideBubble(RepairMineBubble);
			HideBubble(ItemBubble);
		}

		public override void OnOpen() {
			OnItemButtonClicked();
		}

		private void Awake() {
			StartMiningButton.onClick.AddListener(OnStartMiningClicked);
			RepairMineButton.onClick.AddListener(OnRepairMineClicked);
			ItemButton.onClick.AddListener(OnItemButtonClicked);
		}

		private void OnItemButtonClicked() {
			if (ItemModel.Mine == null) {
				return;
			}

			if (ItemModel.Mine.Queue.Count > 0 && ItemModel.Mine.State == MineState.ItemsComplete) {
				MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerClaimMinedItems(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
				);
			}
		}

		private void OnStartMiningClicked() {
			if (ItemModel.Mine.Info.RequiresBuilder && MetaplayClient.PlayerModel.Builders.Free == 0) {
				SignalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.NoBuildersAvailable")));
			} else {
				if (MetaplayClient.PlayerModel.Merge.Energy.ProducedAtUpdate >= ItemModel.Mine.EnergyUsage) {
					MetaplayClient.PlayerContext.ExecuteAction(
						new PlayerUseMine(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
					);
				} else {
					//BuyEnergyPopup.ShowAsync(default).Forget();
					uiRootController.ShowUI<BuyEnergyPopup, BuyEnergyPopupPayload>(new(), CancellationToken.None);
				}
			}
		}

		private void OnRepairMineClicked() {
			if (MetaplayClient.PlayerModel.Builders.Free > 0) {
				MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerRepairMine(applicationInfo.ActiveIsland.Value, ItemModel.X, ItemModel.Y)
				);
			} else {
				SignalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.NoBuildersAvailable")));
			}
		}
	}
}
