using UnityEngine;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class ItemSelectorAddOn : ItemAddOn {

		[SerializeField] private GameObject ItemSelectorFront;
		[SerializeField] private GameObject ItemSelectorBack;
		private bool selected;

		protected override void Setup() {
		}

		public override void OnSelected() {
			SetActive(true);
			selected = true;
		}

		public override void OnDeselected() {
			SetActive(false);
			selected = false;
		}

		public override void OnBeginDrag() {
			SetActive(false);
		}

		public override void OnEndDrag() {
			SetActive(selected);
		}

		public override void OnBeginMove() {
			SetActive(false);
		}

		public override void OnEndMove() {
			SetActive(selected);
		}

		public override void OnOpen() {
			if (Adapter.CanCollect) {
				SetActive(false);
			}
		}

		private void SetActive(bool active) {
			ItemSelectorFront.SetActive(active);
			ItemSelectorBack.SetActive(active);
		}
	}
}
