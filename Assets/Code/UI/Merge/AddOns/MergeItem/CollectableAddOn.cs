using System;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class CollectableAddOn : ItemAddOn {
		[SerializeField] private RectTransform Art;
		[SerializeField] private RectTransform TapIndicator;
		private bool maxLevel;

		protected override void Setup() {
			OnStateChanged();
			maxLevel = MetaplayClient.PlayerModel.GameConfig.ChainMaxLevels.GetMaxLevel(ItemModel.Info.Type) ==
				ItemModel.Info.Level;
		}

		public override void OnStateChanged() {
			TapIndicator.gameObject.SetActive(Adapter.CanCollect);
		}

		private void Update() {
			if (Adapter.CanCollect) {
				TapIndicator.Rotate(0, 0, Time.deltaTime * 50);
				if (maxLevel) {
					Art.localScale = Vector3.one * (1 + Mathf.Sin(Time.time * 5) * 0.1f);
				}
			}
		}
	}
}
