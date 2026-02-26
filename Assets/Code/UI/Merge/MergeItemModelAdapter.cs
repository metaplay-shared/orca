using Code.HeroTasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Code.UI.Application;
using Code.UI.Building;
using Code.UI.Core;
using Code.UI.Effects;
using Code.UI.Hud;
using Code.UI.HudBase;
using Code.UI.InfoMessage.Signals;
using Code.UI.Merge.Hero;
using Code.UI.MergeBase;
using Code.UI.MergeBase.Signals;
using Code.UI.Tasks.Hero;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge {
	public class MergeItemModelAdapter : IMergeItemModelAdapter {
		public object Id => Model;
		public bool HasTimer => Model.HasTimer;
		public bool IsBubble => Model.Bubble;
		public ChainTypeId Type => Model.Info.Type;
		public int Level => Model.Info.Level;
		public float Progression => Model.ConverterProgress.Float;
		public ItemState State => Model.State;
		public ItemBuildState BuildState => Model.BuildState;
		public bool IsUsedInTask =>
			Model.CanMove && MetaplayClient.PlayerModel.Islands[Island].Tasks?.IsItemUsed(Model.Info.ConfigKey) == true;
		public bool UnderLockArea =>
			!MetaplayClient.PlayerModel.Islands[Island].MergeBoard.LockArea.IsFree(Model.X, Model.Y);
		public bool CanCollect => Model.CanCollect(Island);
		public bool CanRemove => Model.CanRemove;
		public bool IsMaxLevel =>
			!MetaplayClient.PlayerModel.GameConfig.Chains.ContainsKey(
				new LevelId<ChainTypeId>(Model.Info.Type, Model.Info.Level + 1)
			);
		public string TimeLeftText =>
			Model.Creator.TimeToFill(MetaplayClient.PlayerModel.CurrentTime).ToSimplifiedString();
		public bool QuickOpen =>
			Model.Info.SelectAction != SelectActionId.None ||
			(IsMaxLevel && Model.Info.CollectableType != CurrencyTypeId.None);

		public int X => Model.X;
		public int Y => Model.Y;
		public int Width => Model.Info.Width;
		public int Height => Model.Info.Height;

		public bool IsHeroItemTarget => Model.Info.HeroTarget;
		public bool IsBuilding => Model.Info.Building;

		public readonly ItemModel Model;
		public readonly IslandTypeId Island;

		[Inject] private SignalBus signalBus;
		[Inject] private MergeBoardRoot mergeBoard;
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private IslandTokenParticles islandTokenParticles;
		[Inject] private IUIRootController uiRootController;
		[Inject] private IFrameRateController frameRateController;
		[Inject] private IHeroTasksFlowController heroTasksFlowController;

		public MergeItemModelAdapter(ItemModel model, IslandTypeId island) {
			Model = model;
			Island = island;
		}

		public void Open() {
			if (CanCollect) {
				signalBus.Fire(new ItemCollectedSignal(X, Y));
				GameObject flyingGo = CopyItem();

				MetaplayClient.PlayerContext.ExecuteAction(new PlayerCollectMergeItem(Island, X, Y));

				FlyItems(flyingGo).Forget();

				return;
			}

			if (Model.Creator != null) {
				if (MetaplayClient.PlayerModel.Merge.Energy.ProducedAtUpdate < Model.Creator.Info.EnergyUsage) {
					OutOfEnergy();
				} else {
					MetaplayClient.PlayerContext.ExecuteAction(new PlayerCreateMergeItem(Island, X, Y));
				}
			}

			if (Model.Info.SelectAction != SelectActionId.None) {
				HandleConfiguredSelectAction(Model.Info.SelectAction);
			}
		}

		private GameObject CopyItem() {
			GameObject topLayer = GameObject.Find("TopLayer");

			MergeItem item = mergeBoard.ItemAt(X, Y);
			GameObject itemGo = item.Handle;
			Sprite sprite = item.ItemSprite;

			GameObject flyingGo = new GameObject();
			Image flyingImage = flyingGo.AddComponent<Image>();
			flyingImage.raycastTarget = false;
			flyingImage.sprite = sprite;
			RectTransform flyingRt = flyingGo.GetComponent<RectTransform>();
			flyingRt.SetParent(topLayer.transform, false);
			flyingRt.sizeDelta = itemGo.GetComponent<RectTransform>().sizeDelta;
			flyingRt.position = itemGo.transform.position;
			return flyingGo;
		}

		public float GetFlightTime(float distance) =>
			MetaplayClient.PlayerModel.GameConfig.Client.GetItemOnBoardFlightTime(distance);

		public async UniTask AcknowledgeBuilding() {
			await UniTask.Yield();
			if (BuildState == ItemBuildState.PendingComplete) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerAcknowledgeBuilding(Island, Model.X, Model.Y));
			}
		}

		public void Select() {
			if (Model.Info.SelectedTriggers.Count > 0) {
				MetaplayClient.PlayerContext.ExecuteAction(
					new PlayerSelectMergeItem(Model.Info.Type, Model.Info.Level)
				);
			}
		}

		public ChainInfo GetNextLevelItemInfo() {
			return Model.GetNextLevelItemInfo(MetaplayClient.PlayerModel.GameConfig);
		}

		private void HandleConfiguredSelectAction(SelectActionId selectAction) {
			if (selectAction == SelectActionId.HeroPopup) {
				heroTasksFlowController.Run(Model.Info.Type, CancellationToken.None).Forget();
			}
		}

		private void OutOfEnergy() {
			uiRootController.ShowUI<BuyEnergyPopup, BuyEnergyPopupPayload>(
				new BuyEnergyPopupPayload(),
				CancellationToken.None
			);
		}

		private async UniTask FlyItems(GameObject objectToFly) {
			if (MetaplayClient.PlayerModel.GameConfig.HeroResources.Contains(Model.Info.CollectableType)) {
				// Wait that the inventory button has change to appear before flying items to it
				await UniTask.Yield();
				await FlyTo<InventoryFlightTarget>(objectToFly);
			} else if (Model.Info.TargetIsland != IslandTypeId.All && Model.Info.TargetIsland != Island) {
				await FlyTo<ShipFlightTarget>(objectToFly);
			} else if (MetaplayClient.PlayerModel.GameConfig.HeroItems.ContainsKey(Model.Info.Type)) {
				HeroTypeId hero = MetaplayClient.PlayerModel.GameConfig.HeroItems[Model.Info.Type];
				ChainTypeId buildingId = MetaplayClient.PlayerModel.Heroes.Heroes[hero].Building;
				await FlyTo(buildingId, objectToFly);
			} else {
				GameObject.Destroy(objectToFly);
			}
		}

		private async UniTask FlyTo(ChainTypeId buildingId, GameObject objectToFly) {
			HeroItemFlightTarget[] targets = Object.FindObjectsOfType<HeroItemFlightTarget>();
			foreach (HeroItemFlightTarget target in targets) {
				if (target.Type == buildingId) {
					using (frameRateController.RequestHighFPS()) {
						await target.FlyFromAsync(objectToFly.GetComponent<RectTransform>());
					}

					return;
				}
			}

			Object.Destroy(objectToFly);
		}

		private async UniTask FlyTo<TFlightTarget>(GameObject objectToFly) where TFlightTarget : FlightTarget {
			TFlightTarget flightTarget = Object.FindObjectOfType<TFlightTarget>();

			if (flightTarget == null) {
				GameObject.Destroy(objectToFly);
				return;
			}

			if (!flightTarget.CanFlyHere) {
				GameObject.Destroy(objectToFly);
				return;
			}

			await flightTarget.FlyFromAsync(objectToFly.GetComponent<RectTransform>());
		}
	}
}
