using System.Linq;
using Code.UI.AssetManagement;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.RequirementsDisplay {
	public class RequirementResourceUiItem : MonoBehaviour {
		[SerializeField] private Image ResourceGraphic;
		[SerializeField] private TMP_Text AmountsText;

		[Inject] private AddressableManager addressableManager;

		public void Setup(RequirementResourceItem source) {
			var currencyTypeId = CurrencyTypeId.FromString(source.TypeName);
			bool playerHasResource = MetaplayClient.PlayerModel.Inventory.Resources.ContainsKey(currencyTypeId);
			int currentAmount = playerHasResource ? MetaplayClient.PlayerModel.Inventory.Resources[currencyTypeId] : 0;

			AmountsText.text = $"{currentAmount}/{source.RequiredAmount}";
			ResourceGraphic.sprite = addressableManager.GetItemIcon(source.TypeName, 1);
		}

		public void Setup(RequirementItemItem source, IslandTypeId islandType) {
			int currentAmount = MetaplayClient.PlayerModel.Islands[islandType].MergeBoard.Items
				.Count(i => i.Info.Type.Value == source.TypeName && i.Info.Level == source.ItemLevel && i.CanMove);
			AmountsText.text = $"{currentAmount}/{source.RequiredAmount}";
			ResourceGraphic.sprite = addressableManager.GetItemIcon(source.TypeName, source.ItemLevel);
		}
	}
}
