using Code.Purchasing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Effects;
using Code.UI.InfoMessage.Signals;
using Code.UI.MergeBase;
using Code.UI.Utils;
using Code.UI.VipPass;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Math;
using Metaplay.Unity.DefaultIntegration;
using Metaplay.Unity.IAP;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeBoard.LockArea {
	public class LockArea : MonoBehaviour {
		[Inject] private MergeBoardRoot mergeBoard;
		[Inject] private SignalBus signalBus;
		[Inject] private AddressableManager addressableManager;
		[Inject] private Canvas canvas;
		[Inject] private CloudParticles cloudParticles;
		[Inject] private IPurchasingFlowController purchasingFlowController;
		[Inject] private IUIRootController uiRootController;

		[SerializeField] private LockAreaState LockAreaState;
		[SerializeField] private GameObject ParticleTarget;

		private bool particleTargetAdded;

		private LockAreaInfo info;

		private bool canOpen;
		private bool isShaking;
		private readonly List<Image> tileImages = new();

		private AreaState State =>
			MetaplayClient.PlayerModel.Islands[info.IslandId].MergeBoard.LockArea.Areas[info.AreaIndex];

		public void Setup(List<LockedTile> areaTiles) {
			info = areaTiles.First().LockAreaInfo;

			RectTransform areaRt = GetComponent<RectTransform>();

			foreach (var tileModel in areaTiles) {
				CreateBlockTile(tileModel, areaRt);
			}

			LockAreaState.transform.SetAsLastSibling();
			SetupTileCenter(areaRt, areaTiles);
			signalBus.Subscribe<AreaStateChangedSignal>(OnAreaStateChanged);
			signalBus.Subscribe<ResourcesChangedSignal>(OnResourcesChanged);
			LockAreaState.Setup(info);
			UpdateState(State);

			if (info.Transparent) {
				foreach (Image tileImage in tileImages) {
					tileImage.color = new Color(1, 1, 1, 0.6f);
				}
			}
		}

		public string HighlightType => $"LockArea{info.Index}";

		private void Update() {
			if (State != AreaState.Opening ||
				!canOpen) {
				return;
			}

			float scale = 1 + Mathf.Sin(Time.frameCount / 10f) * 0.25f;
			LockAreaState.transform.localScale = new Vector3(scale, scale, scale);

			foreach (Image cloud in tileImages) {
				float fade = 1 + Mathf.Sin(Time.frameCount / 10f) * 0.10f;
				var color = cloud.color;
				color.r = fade;
				color.g = fade;
				color.b = fade;
				cloud.color = color;
			}
		}

		private void OnDisable() {
			signalBus.TryUnsubscribe<AreaStateChangedSignal>(OnAreaStateChanged);
			signalBus.TryUnsubscribe<ResourcesChangedSignal>(OnResourcesChanged);
		}

		private void OnAreaStateChanged(AreaStateChangedSignal signal) {
			if (signal.IslandId == info.IslandId &&
				(signal.Index == info.Index || signal.Index == info.Dependency)) {
				UpdateState(signal.State);
			}
		}

		private void UpdateState(AreaState state) {
			switch (State) {
				case AreaState.Locked:
					LockAreaState.ChangeState(State);
					break;
				case AreaState.Opening:
					LockAreaState.ChangeState(State);
					canOpen = EnoughTokens();
					LockAreaState.TapText.SetActive(canOpen);
					if (!particleTargetAdded && info.UnlockCost.Type == CurrencyTypeId.TrophyTokens) {
						GameObject target = Instantiate(ParticleTarget, transform);
						target.layer = LayerMask.NameToLayer("TrophyTokens");
						particleTargetAdded = true;
					}
					break;
				case AreaState.Open:
					SpawnCloudParticles();
					signalBus.Fire(new LockAreaOpenedSignal());
					Destroy(gameObject);
					break;
			}
		}

		private void SpawnCloudParticles() {
			foreach (var cloud in tileImages) {
				cloudParticles.SpawnAt(cloud.transform.position, 10).Forget();
			}
		}

		private void OnResourcesChanged(ResourcesChangedSignal signal) {
			if (signal.ResourceType == CurrencyTypeId.IslandTokens) {
				if (State == AreaState.Opening) {
					canOpen = EnoughTokens();
					LockAreaState.TapText.SetActive(canOpen);
				}
			}
		}

		private bool EnoughTokens() {
			return MetaplayClient.PlayerModel.Wallet.EnoughCurrency(info.UnlockCost.Type, info.UnlockCost.Amount) &&
				info.UnlockProduct == InAppProductId.FromString("None");
		}

		private void SetupTileCenter(RectTransform areaRt, List<LockedTile> areaTiles) {
			Vector3 position = FindCenterPosition(areaRt, areaTiles);
			LockAreaState.transform.position = position;
		}

		private Vector3 FindCenterPosition(Transform area, List<LockedTile> areaTiles) {
			int x = F64.FloorToInt(info.LockX);
			int y = F64.FloorToInt(info.LockY);
			MergeTile tile = mergeBoard.Tiles.FirstOrDefault(t => t.X == x && t.Y == y);
			if (tile == null) {
				throw new Exception($"Invalid coordinates for area {info.Index}: ({x}, {y})");
			}

			RectTransform tileRt = tile.GetComponent<RectTransform>();
			Vector3 targetPosition = tileRt.position;
			Vector3 screenPosition = canvas.worldCamera.WorldToScreenPoint(targetPosition);
			Vector2 size = tileRt.sizeDelta;
			screenPosition += new Vector3(size.x * (info.LockX - x).Float, size.y * (info.LockY - y).Float, 0) *
				canvas.scaleFactor;
			return canvas.worldCamera.ScreenToWorldPoint(screenPosition);
		}

		private void CreateBlockTile(LockedTile tile, Transform parent) {
			GameObject tileGo = new GameObject($"{tile.X}, {tile.Y} ({tile.LockAreaInfo.Index})");
			RectTransform tileRt = tileGo.AddComponent<RectTransform>();
			tileRt.SetParent(parent, false);

			MergeTile targetTile = mergeBoard.Tiles.FirstOrDefault(t => t.X == tile.X && t.Y == tile.Y);
			RectTransform targetRt = targetTile.Handle.GetComponent<RectTransform>();

			tileRt.position = targetRt.position;
			tileRt.sizeDelta = targetRt.sizeDelta;

			Image tileImage = tileGo.AddComponent<Image>();
			tileImage.sprite = ResolveGraphic(tile.X, tile.Y);
			CreateCorner(tile.X, tile.Y, parent, targetRt);

			Button button = tileGo.AddComponent<Button>();
			button.transition = Selectable.Transition.None;
			button.onClick.AddListener(OnAreaClick);

			if (tileGo.TryGetComponent(out Image image)) {
				tileImages.Add(image);
			}
		}

		private void CreateCorner(int tileX, int tileY, Transform parent, RectTransform targetRt) {
			int code = MetaplayClient.PlayerModel.Islands[info.IslandId].MergeBoard.LockArea.GetCloudCode(tileX, tileY);

			int corners = (code & 0xF0) >> 4;
			bool bottomLeft = (corners & 0x1) > 0;
			bool bottomRight = ((corners >> 1) & 0x1) > 0;
			bool topLeft = ((corners >> 2) & 0x1) > 0;
			bool topRight = ((corners >> 3) & 0x1) > 0;

			EdgeBorder(tileX, tileY, bottomLeft, bottomRight, topLeft, topRight, parent, targetRt);
		}

		private Sprite ResolveGraphic(int tileX, int tileY) {
			int code = MetaplayClient.PlayerModel.Islands[info.IslandId].MergeBoard.LockArea.GetCloudCode(tileX, tileY);

			int index = code & 0x0F;

			return addressableManager.Get<Sprite>($"Lockarea/{index}.png");
		}

		private void EdgeBorder(
			int x,
			int y,
			bool bottomLeft,
			bool bottomRight,
			bool topLeft,
			bool topRight,
			Transform parent,
			RectTransform targetRt
		) {
			if (!bottomLeft &&
				!bottomRight &&
				!topLeft &&
				!topRight) {
				return;
			}

			SpawnEdge(bottomLeft, 1);
			SpawnEdge(bottomRight, 2);
			SpawnEdge(topLeft, 3);
			SpawnEdge(topRight, 4);

			void SpawnEdge(bool shouldSpawn, int index) {
				if (!shouldSpawn) {
					return;
				}

				GameObject tileGo = new GameObject($"{x}, {y} ({index})");

				RectTransform tileRt = tileGo.AddComponent<RectTransform>();
				tileRt.SetParent(parent, false);
				tileRt.position = targetRt.position;

				tileRt.sizeDelta = new Vector2(MergeBoardRoot.TILE_WIDTH, MergeBoardRoot.TILE_HEIGHT);

				Image image = tileGo.AddComponent<Image>();

				image.sprite = addressableManager.Get<Sprite>($"Lockarea/c{index}.png");
				image.raycastTarget = false;

				tileImages.Add(image);
			}
		}

		private void OnAreaClick() {
			ShakeAreaAsync(default).Forget();
			LockAreaState.HandleClick();

			if (State == AreaState.Locked) {
				if (MetaplayClient.PlayerModel.Level.Level < info.PlayerLevel) {
					signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.HigherPlayerLevelRequired")));
				} else if (info.Hero != HeroTypeId.None && (!MetaplayClient.PlayerModel.Heroes.Heroes.ContainsKey(info.Hero) ||
							MetaplayClient.PlayerModel.Heroes.Heroes[info.Hero].Level.Level < info.HeroLevel)) {
					signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.HeroRequired")));
				} else {
					signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.PreviousAreaLocked")));
				}
			} else if (State == AreaState.Opening) {
				if (info.UnlockCost.Type == CurrencyTypeId.IslandTokens &&
					MetaplayClient.PlayerModel.Wallet.IslandTokens.Value < info.UnlockCost.Amount) {
					signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.NotEnoughIslandTokens")));
					return;
				} else if (info.UnlockCost.Type == CurrencyTypeId.TrophyTokens &&
					MetaplayClient.PlayerModel.Wallet.TrophyTokens.Value < info.UnlockCost.Amount) {
					signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.NotEnoughTrophyTokens")));
					return;
				}

				if (info.UnlockProduct != InAppProductId.FromString("None")) {
					IAPManager.StoreProductInfo? productMaybe =
						MetaplayClient.IAPManager.TryGetStoreProductInfo(info.UnlockProduct);
					if (productMaybe.HasValue) {
						InAppProductInfo productInfo =
							MetaplayClient.PlayerModel.GameConfig.InAppProducts[info.UnlockProduct];
						if (productInfo.VipPassId == VipPassId.None) {
							HandleAreaPurchase().Forget();
						} else {
							HandleVipPassPurchase().Forget();
						}
					} else {
						signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.ComingSoon")));
					}

					return;
				}

				if (info.UnlockCost.Type == CurrencyTypeId.Gems) {
					purchasingFlowController.TrySpendGemsAsync(info.UnlockCost.Amount, CancellationToken.None)
						.ContinueWith(
							async success => {
								if (success) {
									await UniTask.WaitWhile(
										() => uiRootController.IsAnyUIVisible(),
										PlayerLoopTiming.Update,
										CancellationToken.None
									);

									MetaplayClient.PlayerContext.ExecuteAction(
										new PlayerOpenLockArea(info.IslandId, info.AreaIndex)
									);
								}
							}
						).Forget();
					return;
				}

				MetaplayClient.PlayerContext.ExecuteAction(new PlayerOpenLockArea(info.IslandId, info.AreaIndex));
			}
		}

		private async UniTask HandleAreaPurchase() {
			OfferPopupHandle handle = uiRootController.ShowUI<OfferPopup, OfferPopupHandle>(
				new OfferPopupHandle(info.UnlockProduct, false),
				CancellationToken.None
			);

			OfferPopupResult result = await handle.OnCompleteWithResult;

			if (result.Response != OfferPopupResponse.Yes) {
				signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.PurchaseRequired")));
			}
		}

		private async UniTask HandleVipPassPurchase() {
			VipPassPopupPayload handle = uiRootController.ShowUI<VipPassPopup, VipPassPopupPayload>(
				new VipPassPopupPayload(info.UnlockProduct),
				CancellationToken.None
			);
			await handle.OnComplete;
		}

		private async UniTask ShakeAreaAsync(CancellationToken ct) {
			if (isShaking) {
				return;
			}

			isShaking = true;

			List<UniTask> tasks = new();

			foreach (Transform cloudTransform in transform) {
				UniTask task = cloudTransform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.2f)
					.SetLink(cloudTransform.gameObject)
					.ToUniTask(cancellationToken: ct);
				tasks.Add(task);
			}

			await UniTask.WhenAll(tasks);
			isShaking = false;
		}
	}
}
