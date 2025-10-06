using Code.UI.MergeBase;
using Game.Logic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns {
	public abstract class ItemAddOn : MonoBehaviour {
		protected IMergeItemModelAdapter Adapter { get; private set; }
		protected ItemModel ItemModel { get; private set; }
		protected RectTransform ItemOverlayLayer { get; private set; }
		[Inject] protected SignalBus SignalBus { get; private set; }

		public void Setup(IMergeItemModelAdapter adapter, RectTransform itemOverlayLayer) {
			Adapter = adapter;
			ItemModel = ((MergeItemModelAdapter) adapter).Model;
			ItemOverlayLayer = itemOverlayLayer;
			Setup();
		}

		protected void ShowBubble(RectTransform bubble) {
			bubble.SetParent(gameObject.transform, false);
			bubble.anchoredPosition = Vector3.zero;
			bubble.SetParent(ItemOverlayLayer, true);
			bubble.localScale = Vector3.one;
			bubble.gameObject.SetActive(true);
		}

		protected void HideBubble(RectTransform bubble) {
			if (bubble != null) {
				bubble.SetParent(gameObject.transform, false);
				bubble.gameObject.SetActive(false);
			}
		}

		protected abstract void Setup();

		public virtual bool IsActive => true;

		public virtual void OnItemEnabled() { }
		public virtual void OnItemDisabled() { }

		public virtual void OnSelected() { }
		public virtual void OnDeselected() { }
		public virtual void OnOpen() { }

		public virtual void OnBeginDrag() { }
		public virtual void OnEndDrag() { }
		public virtual void OnBeginMove() { }
		public virtual void OnEndMove() { }
		public virtual void OnStateChanged() { }
		public virtual void OnDestroySelf() { }
		public virtual void OnHoverMergeTarget(bool isHovering, MergeBase.MergeItem mergeItem) { }
	}
}
