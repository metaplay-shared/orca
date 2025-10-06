using Code.UI.Core;
using Code.UI.SendToIsland;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class ConsumableBuilderAddOn : ItemAddOn {
		[SerializeField] private RectTransform ConsumableBuilderBubble;
		[SerializeField] private TMP_Text InfoText;
		[SerializeField] private Button SendToButton;

		[Inject] private IUIRootController uiRootController;

		public override bool IsActive => ItemModel != null && ItemModel.IsBuilder;

		protected override void Setup() {
			if (!IsActive) {
				return;
			}
			InfoText.SetText($"{ItemModel.Booster.BuildTime.ToSimplifiedString()}");
		}

		public override void OnSelected() {
			ShowBubble(ConsumableBuilderBubble);
		}

		public override void OnDeselected() {
			HideBubble(ConsumableBuilderBubble);
		}

		public override void OnBeginDrag() {
			HideBubble(ConsumableBuilderBubble);
		}

		public override void OnDestroySelf() {
			HideBubble(ConsumableBuilderBubble);
		}

		private void Awake() {
			SendToButton.onClick.AddListener(OnSendToClicked);
		}

		private void OnSendToClicked() {
			uiRootController.ShowUI<SendToIslandPopup, SendToIslandPopupHandle>(
				new SendToIslandPopupHandle(ItemModel),
				CancellationToken.None
			);
			Debug.Log("ConsumableBuilderAddOn.OnSendToClicked");
		}
	}
}
