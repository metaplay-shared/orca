using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity.DefaultIntegration;
using UniRx;

namespace Code.Logbook {
	public interface IItemDiscoveryController {
		IReadOnlyReactiveProperty<bool> HasPendingRewards { get; }

		void OnItemDiscoveryChanged(LevelId<ChainTypeId> chainId);
		bool IsItemDiscovered(LevelId<ChainTypeId> itemId);
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class ItemDiscoveryController : IItemDiscoveryController {
		private readonly ReactiveProperty<bool> hasPendingRewards;

		public ItemDiscoveryController() {
			hasPendingRewards = new ReactiveProperty<bool>(HasPendingItemDiscoveryRewards());
		}

		public void OnItemDiscoveryChanged(LevelId<ChainTypeId> chainId) {
			UpdateHasPendingItemDiscoveryRewards();
		}

		public bool IsItemDiscovered(LevelId<ChainTypeId> itemId) {
			var discoveries = MetaplayClient.PlayerModel.Merge.ItemDiscovery.Discovery;
			if (!discoveries.TryGetValue(itemId, out DiscoveryStatus status)) {
				return false;
			}

			bool isDiscovered = status.State is DiscoveryState.Discovered or DiscoveryState.Claimed;
			return isDiscovered;
		}

		public IReadOnlyReactiveProperty<bool> HasPendingRewards => hasPendingRewards;

		private void UpdateHasPendingItemDiscoveryRewards() {
			hasPendingRewards.Value = HasPendingItemDiscoveryRewards();
		}

		private bool HasPendingItemDiscoveryRewards() {
			return MetaplayClient.PlayerModel.Merge.ItemDiscovery.SomethingToClaim(MetaplayClient.PlayerModel.GameConfig);
		}
	}
}
