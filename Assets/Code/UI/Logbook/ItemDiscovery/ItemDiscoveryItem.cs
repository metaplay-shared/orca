using Code.Logbook;
using System;
using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.Events;
using Code.UI.ItemDiscovery.Signals;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.Model;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.ItemDiscovery {
	public class ItemDiscoveryItem : MonoBehaviour {
		[SerializeField] private Image ItemIcon;
		[SerializeField] private TMP_Text NameLabelText;
		[SerializeField] private Button ClaimButton;
		[SerializeField] private RectTransform FloatingClaimedRewardLocation;
		[SerializeField] private Color NotDiscoveredColor = Color.black;
		[SerializeField] private Color DiscoveredColor = Color.white;

		[Inject] private AddressableManager addressableManager;
		[Inject] private SignalBus signalBus;
		[Inject] private FloatingClaimedReward floatingClaimedRewardPrefab;
		[Inject(Id="FloatingClaimedRewardContainer")] private RectTransform floatingClaimedRewardContainer;
		[Inject] private IItemDiscoveryController discoveryController;

		private LevelId<ChainTypeId> chainId;
		private DiscoveryState state;

		protected void OnEnable() {
			signalBus.Subscribe<ItemDiscoveryStateChangedSignal>(OnButtonUpdated);
		}

		protected void OnDisable() {
			signalBus.Unsubscribe<ItemDiscoveryStateChangedSignal>(OnButtonUpdated);
		}

		public void Setup(LevelId<ChainTypeId> id) {
			chainId = id;
			NameLabelText.text = discoveryController.IsItemDiscovered(id)
				? chainId.Localize()
				: Localizer.Localize("Discovery.NotDiscoveredItemName");
			state = MetaplayClient.PlayerModel.Merge.ItemDiscovery.GetState(id);
			ChainInfo chainInfo = MetaplayClient.PlayerModel.GameConfig.Chains[chainId];
			ItemIcon.sprite = addressableManager.GetItemIcon(chainInfo);

			ClaimButton.gameObject.SetActive(state == DiscoveryState.Discovered);
			ClaimButton.onClick.RemoveAllListeners();
			ClaimButton.onClick.AddListener(OnClaimButtonClick);
			ItemIcon.color = state == DiscoveryState.NotDiscovered ? NotDiscoveredColor : DiscoveredColor;
		}

		private void OnClaimButtonClick() {
			MetaActionResult result =
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerClaimItemDiscoveryReward(chainId));
			if (result == MetaActionResult.Success) {
				ClaimRewardAnimation(chainId, this.GetCancellationTokenOnDestroy()).Forget();
			}
		}

		private async UniTask ClaimRewardAnimation(LevelId<ChainTypeId> item, CancellationToken ct) {
			foreach (ResourceInfo resource in MetaplayClient.PlayerModel.GameConfig.Chains[item].DiscoveryRewards) {
				FloatingClaimedReward floatingClaimedReward = Instantiate(
					floatingClaimedRewardPrefab,
					floatingClaimedRewardContainer
				);
				floatingClaimedReward.transform.position = FloatingClaimedRewardLocation.position;
				floatingClaimedReward.gameObject.SetActive(true);
				floatingClaimedReward.Show(
					$"+ <sprite name={resource.Type}> {resource.Amount}"
				);
				await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: ct);
			}
		}

		private void OnButtonUpdated(ItemDiscoveryStateChangedSignal signal) {
			if (signal.ChainId.Equals(chainId)) {
				Setup(chainId);
			}
		}
	}
}
