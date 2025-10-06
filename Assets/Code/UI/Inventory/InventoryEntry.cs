using Code.UI.AssetManagement;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Code.UI.Inventory {
	public class InventoryEntry : MonoBehaviour {
		[SerializeField] private Image Icon;
		[SerializeField] private TMP_Text ResourceName;
		[SerializeField] private TMP_Text ResourceCount;

		private AsyncOperationHandle<Sprite> iconHandle;

		public void Setup(CurrencyTypeId resourceType) {
			ResourceName.text = Localizer.Localize($"Chain.{resourceType.Value}");
			ResourceCount.text = MetaplayClient.PlayerModel.Inventory.Resources[resourceType].ToString();
			SetupIconAsync(resourceType).Forget();
		}

		private async UniTask SetupIconAsync(CurrencyTypeId resourceType) {
			string iconAddress = AddressableUtils.GetItemIconAddress(resourceType.Value, 1);
			iconHandle = Addressables.LoadAssetAsync<Sprite>(iconAddress);
			Icon.sprite = await iconHandle;
		}

		private void OnDestroy() {
			if (iconHandle.IsValid()) {
				Addressables.Release(iconHandle);
			}
		}
	}
}
