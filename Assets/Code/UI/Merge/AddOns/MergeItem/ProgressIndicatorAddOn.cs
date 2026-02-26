using Game.Logic;
using UnityEngine;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class ProgressIndicatorAddOn : ItemAddOn {
		[SerializeField] private ProgressIndicator Indicator;
		private bool selected;

		protected override void Setup() {
			if (IsActive) {
				OnStateChanged();
			}
			Indicator.transform.localScale = Vector3.one * ItemModel.Info.Width;
		}

		public override bool IsActive =>
			ItemModel?.Mine?.State is MineState.Idle or MineState.Mining or MineState.ItemsComplete &&
			!ItemModel.Mine.Info.Chest &&
			ItemModel.Mine.Info.MineCycles > 1 &&
			ItemModel?.State is not ItemState.Hidden or ItemState.PartiallyVisible;

		public override void OnStateChanged() {
			if (!IsActive) {
				Indicator.gameObject.SetActive(false);
				selected = false;
				return;
			}
			Indicator.SetProgress(ItemModel.Mine.MineCycle, ItemModel.Mine.Info.MineCycles);
		}

		public override void OnSelected() {
			Indicator.gameObject.SetActive(true);
			selected = true;
		}

		public override void OnDeselected() {
			Indicator.gameObject.SetActive(false);
			selected = false;
		}

		public override void OnBeginDrag() {
			Indicator.gameObject.SetActive(false);
		}

		public override void OnEndDrag() {
			Indicator.gameObject.SetActive(selected);
		}

		public override void OnBeginMove() {
			Indicator.gameObject.SetActive(false);
		}

		public override void OnEndMove() {
			Indicator.gameObject.SetActive(selected);
		}

		public override void OnOpen() {
			if (Adapter.CanCollect) {
				Indicator.gameObject.SetActive(false);
			}
		}
	}
}
