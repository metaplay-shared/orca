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

namespace Code.UI.Events.SeasonalEvent {
	public class SeasonEventUIRootHandle : UIHandleBase {
		public SeasonalEventModel Model { get; private set; }

		public SeasonEventUIRootHandle(SeasonalEventModel model) {
			Model = model;
		}
	}

	public class SeasonalEventUIRoot : UIRootBase<SeasonEventUIRootHandle> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private EventTimer Timer;
		[SerializeField] private Image[] Step1;
		[SerializeField] private Image[] Step2;
		[SerializeField] private Image Step3;

		[Inject] private AddressableManager addressableManager;

		protected override void Init() {
			ChainTypeId infoChestType = UIHandle.Model.Info.ChestType;
			Step1[0].sprite = LoadSprite(infoChestType, 1);
			Step1[1].sprite = LoadSprite(infoChestType, 2);
			Step1[2].sprite = LoadSprite(infoChestType, 3);

			ChainTypeId infoItemType = UIHandle.Model.Info.ItemType;
			int maxLevel = MetaplayClient.PlayerModel.GameConfig.ChainMaxLevels.GetMaxLevel(infoItemType);
			Step2[0].sprite = LoadSprite(infoItemType, 1);
			Step2[1].sprite = LoadSprite(infoItemType, maxLevel * 1 / 3);
			Step2[2].sprite = LoadSprite(infoItemType, maxLevel * 2 / 3);
			Step2[3].sprite = LoadSprite(infoItemType, maxLevel);

			Step3.SetSpriteFromAddressableAssetsAsync(
				$"Islands/{IslandTypeId.TrophyIsland}.png",
				gameObject.GetCancellationTokenOnDestroy()
			).Forget();

			Timer.Setup(UIHandle.Model.ActivableParams);
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
