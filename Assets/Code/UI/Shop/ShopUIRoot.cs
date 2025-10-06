using Code.UI.Core;
using Code.UI.Market;
using Cysharp.Threading.Tasks;
using Game.Logic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Shop {
	public class ShopUIRoot : UIRootBase<ShopUIHandle> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private ToggleGroup ToggleGroup;
		[SerializeField] private Toggle MarketToggle;
		[SerializeField] private Toggle ShopToggle;
		[SerializeField] private MarketContentPresenter MarketContent;

		protected override void Init() {
			ToggleGroup.SetAllTogglesOff();
			if (UIHandle.NavigationPayload is ShopUIHandle.MarketNavigationPayload marketNavigationPayload) {
				MarketToggle.isOn = true;
				MarketContent.ShowCategory(marketNavigationPayload.CategoryId, UICancellationToken).Forget();
			} else if (UIHandle.NavigationPayload is ShopUIHandle.ShopNavigationPayload) {
				ShopToggle.isOn = true;
			} else {
				MarketToggle.isOn = true;
			}

			MetaplayClient.PlayerContext.ExecuteAction(new PlayerRegisterShopOpenAction());
		}

		protected override UniTask Hide(CancellationToken ct) {
			MetaplayClient.PlayerContext.ExecuteAction(new PlayerRegisterShopCloseAction());
			return base.Hide(ct);
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
	}

	public class ShopUIHandle : UIHandleBase {
		public NavigationPayloadBase NavigationPayload { get; }

		public ShopUIHandle(
			NavigationPayloadBase navigationPayload
		) {
			NavigationPayload = navigationPayload;
		}

		public class MarketNavigationPayload : NavigationPayloadBase {
			public ShopCategoryId CategoryId { get; }

			public MarketNavigationPayload(ShopCategoryId categoryId) {
				CategoryId = categoryId;
			}
		}

		public class ShopNavigationPayload : NavigationPayloadBase { }

		public abstract class NavigationPayloadBase { }
	}
}
