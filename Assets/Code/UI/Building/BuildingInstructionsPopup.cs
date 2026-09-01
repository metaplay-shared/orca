using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Building {
	public class BuildingInstructionsPopup : UIRootBase<BuildingPopupPayload> {
		[SerializeField] private Button CloseButton;

		[SerializeField] private Image[] Step1;
		[SerializeField] private Image[] Step2;
		[SerializeField] private Image Step3;

		[Inject] private AddressableManager addressableManager;

		protected override void Init() {
			Step1[0].sprite = LoadSprite(UIHandle.ItemType, 1);
			Step1[1].sprite = LoadSprite(UIHandle.ItemType, 2);
			Step1[2].sprite = LoadSprite(UIHandle.ItemType, 4);
			Step1[3].sprite = LoadSprite(UIHandle.ItemType, 6);

			int maxLevel = MetaplayClient.PlayerModel.GameConfig.ChainMaxLevels.GetMaxLevel(UIHandle.ItemType);
			Sprite maxLevelSprite = LoadSprite(UIHandle.ItemType, maxLevel);
			foreach (Image image in Step2) {
				image.sprite = maxLevelSprite;
			}

			Step3.sprite = LoadSprite(UIHandle.BuildingType, 2);
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

		private Sprite LoadSprite(ChainTypeId type, int level) {
			var chainInfo = MetaplayClient.PlayerModel.GameConfig.Chains[new LevelId<ChainTypeId>(type, level)];
			return addressableManager.GetItemIcon(chainInfo);
		}
	}
}
