using System;
using Code.UI.MergeBase.Signals;
using UnityEngine;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class PointerAddOn : ItemAddOn {
		[SerializeField] private RectTransform Pointer;

		private bool visible = false;
		private Vector3 localPosition;

		protected override void Setup() {
		}

		public override void OnItemEnabled() {
			SignalBus.Subscribe<PointItemSignal>(ShowPointer);
		}

		public override void OnItemDisabled() {
			SignalBus.Unsubscribe<PointItemSignal>(ShowPointer);
		}

		private void Update() {
			if (Pointer == null || !Pointer.gameObject.activeSelf) {
				return;
			}

			float diff = Math.Abs(Mathf.Sin(Time.frameCount / 10f) * 0.3f) * 0.5f;
			Pointer.transform.position = transform.position + localPosition + new Vector3(0, diff, 0);
		}

		public override void OnSelected() {
			Pointer.transform.SetParent(gameObject.transform, false);
			Pointer.gameObject.SetActive(false);
			visible = false;
		}

		public override void OnOpen() {
			Pointer.transform.SetParent(gameObject.transform, false);
			Pointer.gameObject.SetActive(false);
			visible = false;
		}

		public override void OnBeginDrag() {
			Pointer.gameObject.SetActive(false);
		}

		public override void OnEndDrag() {
			if (visible) {
				Show();
			}
		}

		public override void OnBeginMove() {
			Pointer.gameObject.SetActive(false);
		}

		public override void OnEndMove() {
			if (visible) {
				Show();
			}
		}

		public override void OnDestroySelf() {
			if (Pointer != null) {
				Destroy(Pointer.gameObject);
			}
		}

		private void Show()
		{
			if (Pointer == null)
				return;
			
			Pointer.SetParent(transform, false);
			Pointer.anchoredPosition = Vector2.zero;
			localPosition = Pointer.position - transform.position;
			Pointer.SetParent(ItemOverlayLayer, true);
			Pointer.gameObject.SetActive(true);
			visible = true;
		}

		private void ShowPointer(PointItemSignal signal) {
			if (signal.X == Adapter.X && signal.Y == Adapter.Y) {
				Show();
			}
		}
	}
}
