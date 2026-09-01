using Game.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class CreatorAddOn : ItemAddOn {

		[SerializeField] private Image Creator;
		protected override void Setup() {
			OnStateChanged();
		}

		private void Update() {
			var color = Creator.color;
			color.a = Mathf.Sin(Time.realtimeSinceStartup * 4 + ItemModel.X);
			Creator.color = color;
		}

		public override void OnStateChanged() {
			Creator.gameObject.SetActive(
				ItemModel.CanCreate && !ItemModel.HasTimer ||
				ItemModel.Mine?.State == MineState.Idle && ItemModel.Info.Width == 1 && ItemModel.State == ItemState.Free);
		}
	}
}
