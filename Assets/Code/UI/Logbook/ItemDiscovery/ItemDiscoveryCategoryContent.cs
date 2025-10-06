using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using Orca.Unity.Utilities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Code.UI.ItemDiscovery {
	public class ItemDiscoveryCategoryContent : MonoBehaviour {
		[SerializeField] private ItemDiscoveryItem TemplateItem;
		[SerializeField] private ItemDiscoveryChain TemplateChain;
		[SerializeField] private float CategoryAnimationInitialDelay = 0.1f;
		[SerializeField] private float CategoryAnimationIntervalDelay = 0.05f;
		[SerializeField] private float CategoryAnimationDuration = 0.1f;
		[SerializeField] private float ItemAnimationInitialDelay = 0.1f;
		[SerializeField] private float ItemAnimationIntervalDelay = 0.05f;
		[SerializeField] private float ItemAnimationDuration = 0.1f;

		private CategoryId category;
		private DiContainer container;

		private readonly Dictionary<ChainTypeId, ItemDiscoveryChain> chains = new();
		private readonly Dictionary<LevelId<ChainTypeId>, ItemDiscoveryItem> items = new();

		[Inject]
		[SuppressMessage("ReSharper", "ParameterHidesMember")]
		private void Inject(
			DiContainer container,
			CategoryId category
		) {
			this.container = container;
			this.category = category;
		}

		protected void OnEnable() {
			SpawnCategoryContent(gameObject.GetCancellationTokenOnDestroy()).Forget();
		}

		private async UniTask SpawnCategoryContent(CancellationToken ct) {
			var chainsCategories = MetaplayClient.PlayerModel.GameConfig.ChainCategories.GetValueOrDefault(category);
			if (chainsCategories == null) {
				return;
			}

			for (var i = 0; i < chainsCategories.ToArray().Length; i++) {
				ChainTypeId chainType = chainsCategories.ToArray()[i];
				ItemDiscoveryChain chain = GetOrSpawnSpawnChain(chainType, transform);
				chain.GetOrAddComponent<CanvasGroup>()
					.DOFade(1f, CategoryAnimationDuration)
					.ChangeStartValue(0f)
					.SetDelay(CategoryAnimationInitialDelay + i * CategoryAnimationIntervalDelay);
				int maxLevel = MetaplayClient.PlayerModel.GameConfig.ChainMaxLevels.GetMaxLevel(chainType);
				for (int level = 1; level <= maxLevel; level++) {
					var item = GetOrSpawnItem(chainType, level, chain.Content);
					item.GetOrAddComponent<CanvasGroup>()
						.DOFade(1f, ItemAnimationDuration)
						.ChangeStartValue(0f)
						.SetDelay(ItemAnimationInitialDelay + (level - 1) * ItemAnimationIntervalDelay);
				}

				await UniTask.DelayFrame(1, cancellationToken: ct);
			}
		}

		private ItemDiscoveryChain GetOrSpawnSpawnChain(ChainTypeId chainType, Transform parent) {
			if (!chains.TryGetValue(chainType, out ItemDiscoveryChain chain)) {
				chain = container.InstantiatePrefabForComponent<ItemDiscoveryChain>(TemplateChain, parent);
				chain.Setup(chainType);
				chains.Add(chainType, chain);
			}

			return chain;
		}

		private ItemDiscoveryItem GetOrSpawnItem(ChainTypeId chainType, int level, Transform parent) {
			LevelId<ChainTypeId> itemId = new (chainType, level);
			if (!items.TryGetValue(itemId, out ItemDiscoveryItem item)) {
				item = container.InstantiatePrefabForComponent<ItemDiscoveryItem>(TemplateItem, parent);
				item.Setup(itemId);
				items.Add(itemId, item);
			}

			return item;
		}
	}
}
