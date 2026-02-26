using Cysharp.Threading.Tasks;
using Game.Logic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Code.UI.RequirementsDisplay {
	public class ResourceRequirements : MonoBehaviour {
		[SerializeField] private RectTransform ItemContainer;

		[Inject] private DiContainer container;

		private bool isSettingUp;

		public async UniTask SetupAsync(RequirementResourceItem[] content) {
			Clear();

			GameObject requirementItemGo =
				await Addressables.LoadAssetAsync<GameObject>("RequirementsDisplay/RequirementResourceUiItem");
			RequirementResourceUiItem requirementUiItem = requirementItemGo.GetComponent<RequirementResourceUiItem>();

			foreach (RequirementResourceItem item in content) {
				RequirementResourceUiItem instance = Instantiate(requirementUiItem, ItemContainer);
				container.Inject(instance);
				instance.Setup(item);
			}
		}

		public async UniTask SetupAsync(RequirementItemItem[] content, IslandTypeId islandType) {
			if (isSettingUp) {
				Debug.LogWarning("Setup called multiple times");
				return;
			}

			isSettingUp = true;
			Clear();

			GameObject requirementItemGo =
				await Addressables.LoadAssetAsync<GameObject>("RequirementsDisplay/RequirementResourceUiItem");
			RequirementResourceUiItem requirementUiItem = requirementItemGo.GetComponent<RequirementResourceUiItem>();

			foreach (RequirementItemItem item in content) {
				RequirementResourceUiItem instance = Instantiate(requirementUiItem, ItemContainer);
				container.Inject(instance);
				instance.Setup(item, islandType);
			}

			isSettingUp = false;
		}

		private void Clear() {
			foreach (Transform child in ItemContainer) {
				Destroy(child.gameObject);
			}
		}
	}
}
