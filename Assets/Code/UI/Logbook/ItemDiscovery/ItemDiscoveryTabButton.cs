using Code.UI.AssetManagement;
using Code.UI.ItemDiscovery.Signals;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.ItemDiscovery {
	public class ItemDiscoveryTabButton : MonoBehaviour {
		[SerializeField] private Image Icon;
		[SerializeField] private GameObject NewIndicator;
		[SerializeField] public Toggle Toggle;

		[Inject] private CategoryId category;
		[Inject] private SignalBus signalBus;

		protected void Awake() {
			SetupCategoryIcon();
			UpdateNewIndicator();
		}

		protected void OnEnable() {
			signalBus.Subscribe<ItemDiscoveryStateChangedSignal>(OnStateChange);
		}

		protected void OnDisable() {
			signalBus.Unsubscribe<ItemDiscoveryStateChangedSignal>(OnStateChange);
		}

		private void OnStateChange() {
			UpdateNewIndicator();
		}

		private void SetupCategoryIcon() {
			Icon.SetSpriteFromAddressableAssetsAsync(
				AddressableUtils.GetItemCategoryIconAddress(category),
				gameObject.GetCancellationTokenOnDestroy()
			).Forget();
		}

		private void UpdateNewIndicator() {
			NewIndicator.SetActive(
				MetaplayClient.PlayerModel.Merge.ItemDiscovery.SomethingToClaimInCategory(
					MetaplayClient.PlayerModel.GameConfig,
					category
				)
			);
		}
	}
}
