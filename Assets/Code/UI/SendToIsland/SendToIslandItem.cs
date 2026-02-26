using Code.UI.Application;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.SendToIsland {
	public class SendToIslandItem : MonoBehaviour {
		[SerializeField] private TMP_Text IslandLabel;
		[SerializeField] private Button SendToButton;

		[Inject] protected ApplicationInfo ApplicationInfo;

		private IslandTypeId islandTypeId;
		private ItemModel itemModel;

		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		public void Setup(IslandTypeId islandTypeId, ItemModel itemModel) {
			this.islandTypeId = islandTypeId;
			this.itemModel = itemModel;

			IslandLabel.SetText(Localizer.Localize(islandTypeId));
		}

		public async UniTask OnSendToIslandAsync(CancellationToken ct) {
			await SendToButton.OnClickAsync(ct);
			PlayerSendItemToIsland action = new(
				ApplicationInfo.ActiveIsland.Value,
				itemModel.X,
				itemModel.Y,
				islandTypeId
			);
			action.Execute(MetaplayClient.PlayerModel, true);
		}
	}
}
