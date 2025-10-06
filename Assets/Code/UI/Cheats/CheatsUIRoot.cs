using Code.UI.Application;
using Code.UI.Core;
using Code.UI.Events;
using Code.UI.Shop;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Cheats {
	public class CheatsUIRoot : UIRootBase<CheatsUIHandle> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private CheatItemSpawnButton TemplateItemSpawnButton;
		[SerializeField] private RectTransform ItemSpawnButtonContainer;
		[SerializeField] private RectTransform CheatButtonContainer;

		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private DiContainer container;
		[Inject] private IUIRootController uiRootController;
		#if UNITY_EDITOR || DEVELOPMENT_BUILD
		[Inject] private IFrameRateDebugger frameRateDebugger;
		#endif

		protected override void Init() {
			SpawnItemButtons();
			SpawnCheatButtons();
		}

		protected override UniTask Idle(CancellationToken ct) {
			return UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		private void SpawnItemButtons() {
			foreach (var item in MetaplayClient.PlayerModel.GameConfig.Chains.Keys) {
				var button = container.InstantiatePrefab(TemplateItemSpawnButton, ItemSpawnButtonContainer)
					.GetComponent<CheatItemSpawnButton>();
				button.Setup(item);
			}
		}

		private void SpawnCheatButtons() {
			ResourceButton(CurrencyTypeId.Energy, 10);
			ResourceButton(CurrencyTypeId.Gems, 10);
			ResourceButton(CurrencyTypeId.Gold, 10);
			ResourceButton(CurrencyTypeId.Xp, 10);
			ResourceButton(CurrencyTypeId.IslandTokens, 10);

			CheatButton(
				"Open map",
				() => {
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.Map));
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.HomeIsland));
				}
			);

			CheatButton(
				"Enable Events",
				() => {
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.DiscountEvents));
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.ActivityEvents));
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.DailyTaskEvents));
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.SeasonalEvents));
				}
			);

			CheatButton(
				"Enable Shop",
				() => { MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.Shop)); }
			);

			CheatButton(
				"Enable Logbook",
				() => { MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.Logbook)); }
			);

			CheatButton(
				"Enable Removal",
				() => { MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.ItemRemoval)); }
			);

			CheatButton(
				"Clear board",
				() => MetaplayClient.PlayerContext.ExecuteAction(new CheatClearBoard(applicationInfo.ActiveIsland.Value))
			);

			CheatButton(
				"Shop",
				() => {
					uiRootController.ShowUI<ShopUIRoot, ShopUIHandle>(
						new ShopUIHandle(new ShopUIHandle.ShopNavigationPayload()),
						CancellationToken.None
					);
				}
			);

			CheatButton(
				"Events popup",
				() => { uiRootController.ShowUI<EventsPopup, EventsPopupUIHandle>(new(), CancellationToken.None); }
			);

			CheatButton(
				"Out of energy",
				() => { uiRootController.ShowUI<BuyEnergyPopup, BuyEnergyPopupPayload>(new(), CancellationToken.None); }
			);

			CheatButton(
				"Enable HUD buttons",
				() => {
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.HudButtonGold));
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.HudButtonGems));
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.HudButtonEnergy));
					MetaplayClient.PlayerContext.ExecuteAction(new CheatUnlockFeature(FeatureTypeId.HudButtonBuilders));
					MetaplayClient.PlayerContext.ExecuteAction(
						new CheatUnlockFeature(FeatureTypeId.HudButtonIslandTokens)
					);
				}
			);

			#if UNITY_EDITOR || DEVELOPMENT_BUILD
			CheatButton(
				"Toggle Frame Rate Debugger",
				() => frameRateDebugger.ToggleActive()
			);
			#endif

			CheatButton(
				"Crash game (Abort)",
				() => UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.Abort)
			);

			CheatButton(
				"Crash game (Access violation)",
				() => UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.AccessViolation)
			);

			CheatButton(
				"Crash game (Fatal error)",
				() => UnityEngine.Diagnostics.Utils.ForceCrash(ForcedCrashCategory.FatalError)
			);

			void ResourceButton(CurrencyTypeId currency, int amount) {
				CheatButton(
					$"Add {amount} {currency.Value}",
					() => {
						MetaplayClient.PlayerContext.ExecuteAction(
							new CheatAddResources(
								currency,
								amount
							)
						);
					}
				);
			}

			void CheatButton(string cheatName, UnityAction action) {
				GameObject buttonGo = new GameObject("Cheat button");
				buttonGo.AddComponent<RectTransform>();
				buttonGo.transform.SetParent(CheatButtonContainer, false);
				buttonGo.AddComponent<Image>();
				Button cheatButton = buttonGo.AddComponent<Button>();
				cheatButton.onClick.AddListener(action);

				GameObject textGo = new GameObject("ButtonText");
				RectTransform textRt = textGo.AddComponent<RectTransform>();
				TMP_Text text = textGo.AddComponent<TextMeshProUGUI>();
				text.color = Color.black;
				text.text = cheatName;

				textRt.SetParent(buttonGo.transform, false);
			}
		}
	}

	public class CheatsUIHandle : UIHandleBase { }
}
