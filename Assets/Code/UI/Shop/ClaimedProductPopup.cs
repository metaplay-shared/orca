using System.Threading;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Game.Logic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using RewardItem = Code.UI.Rewarding.RewardItem;

namespace Code.UI.Shop {

	public class ClaimedProductPopupPayload : UIHandleBase {
		public InAppProductInfo Product { get; }

		public ClaimedProductPopupPayload(InAppProductInfo product) {
			Product = product;
		}
	}

	public class ClaimedProductPopup : UIRootBase<ClaimedProductPopupPayload> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private RewardItem RewardTemplate;
		[SerializeField] private RectTransform Products;
		[SerializeField] private RectTransform Resources;
		[SerializeField] private RectTransform Items;
		[Inject] private DiContainer diContainer;

		protected override void Init() {
			bool hasResources = UIHandle.Product.Resources.Count > 0;
			bool hasItems = UIHandle.Product.Items.Count > 0;
			Products.gameObject.SetActive(hasResources || hasItems);
			Resources.gameObject.SetActive(hasResources);
			Items.gameObject.SetActive(hasItems);

			foreach (ResourceInfo resourceInfo in UIHandle.Product.Resources) {
				RewardItem resource = diContainer.InstantiatePrefabForComponent<RewardItem>(RewardTemplate, Resources);
				resource.Setup(resourceInfo.Type, resourceInfo.Amount);
			}

			foreach (ItemCountInfo itemInfo in UIHandle.Product.Items) {
				RewardItem item = diContainer.InstantiatePrefabForComponent<RewardItem>(RewardTemplate, Items);
				item.Setup(itemInfo.Type, itemInfo.Level, itemInfo.Count);
			}
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}
	}
}
