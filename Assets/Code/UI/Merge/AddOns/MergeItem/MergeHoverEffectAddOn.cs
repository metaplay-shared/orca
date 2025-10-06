using Code.UI.AssetManagement;
using Game.Logic;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class MergeHoverEffectAddOn : ItemAddOn {
		[SerializeField] private GameObject EffectObject;
		[SerializeField] private Material ItemParticleEffectMaterial;

		[Inject] private AddressableManager addressableManager;

		private IReactiveProperty<bool> isHoveringMergeTarget;
		private IReactiveProperty<MergeBase.MergeItem> hoveredItem;
		private IReadOnlyReactiveProperty<ChainInfo> nextLevelItemInfo;

		private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

		private void Awake() {
			isHoveringMergeTarget = new ReactiveProperty<bool>(false);
			hoveredItem = new ReactiveProperty<MergeBase.MergeItem>(null);
			nextLevelItemInfo = hoveredItem
				.Select(item => item != null ? item.Adapter.GetNextLevelItemInfo() : null)
				.ToReadOnlyReactiveProperty();
			isHoveringMergeTarget.Subscribe(SetEffectActive).AddTo(gameObject);
			nextLevelItemInfo.Subscribe(SetNextLevelItemEffect).AddTo(gameObject);
		}

		protected override void Setup() {
			isHoveringMergeTarget.Value = false;
			hoveredItem.Value = null;
		}

		public override void OnEndDrag() {
			isHoveringMergeTarget.Value = false;
			hoveredItem.Value = null;
		}

		public override void OnHoverMergeTarget(
			bool isHovering,
			MergeBase.MergeItem mergeItem
		) {
			isHoveringMergeTarget.Value = isHovering;
			hoveredItem.Value = mergeItem;
		}

		private void SetEffectActive(bool active) {
			EffectObject.SetActive(active);
		}

		private void SetNextLevelItemEffect([CanBeNull] ChainInfo itemInfo) {
			if (itemInfo == null) {
				return;
			}

			Sprite sprite = addressableManager.GetItemIcon(itemInfo);
			Texture2D texture = sprite.texture;
			ItemParticleEffectMaterial.SetTexture(MainTexId, texture);
			Vector2 textureSize = new (sprite.texture.width, sprite.texture.height);
			ItemParticleEffectMaterial.SetTextureOffset(MainTexId, sprite.textureRect.position / textureSize);
			ItemParticleEffectMaterial.SetTextureScale(MainTexId, sprite.textureRect.size / textureSize);
		}
	}
}
