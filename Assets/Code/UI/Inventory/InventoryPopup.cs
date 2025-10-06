using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Inventory {
	public class InventoryPopupPayload : UIHandleBase { }

	public class InventoryPopup : UIRootBase<InventoryPopupPayload> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private RectTransform EntryContainer;
		[SerializeField] private InventoryEntry InventoryEntry;

		private DiContainer container;

		[Inject]
		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		private void Inject(DiContainer container) {
			this.container = container;
		}

		protected override void Init() {
			foreach (CurrencyTypeId currencyType in MetaplayClient.PlayerModel.Inventory.Resources.Keys) {
				InventoryEntry entryInstance =
					container.InstantiatePrefabForComponent<InventoryEntry>(InventoryEntry, EntryContainer);
				entryInstance.Setup(currencyType);
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
