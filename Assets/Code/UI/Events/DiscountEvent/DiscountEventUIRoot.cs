using System.Collections.Generic;
using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Events.AdvertisementContents;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events.DiscountEvent {
	public class DiscountEventUIHandle : UIHandleBase {
		public IEventModel EventModel { get; }

		public DiscountEventUIHandle(IEventModel eventModel) {
			this.EventModel = eventModel;
		}
	}

	public class DiscountEventUIRoot : UIRootBase<DiscountEventUIHandle> {
		[SerializeField] private Button CloseButton;
		[SerializeField] private Button OkButton;

		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text Description;
		[SerializeField] private EventTimer Timer;
		[SerializeField] private RectTransform EventContent;

		[SerializeField] private DiscountEventAdvertisementContent PrefabDiscountEventAdvertisementContent;

		[Inject] private AddressableManager addressableManager;
		[Inject] private DiContainer diContainer;

		private DiscountEventModel model;

		protected override void Init() {
			model = (DiscountEventModel)UIHandle.EventModel;

			string eventTranslationIdPrefix = $"Event.Info.Discount.{model.Info.DiscountEventType}";
			string titleText = Localizer.Localize(eventTranslationIdPrefix + ".Title");
			string eventDescription = Localizer.Localize(eventTranslationIdPrefix + ".Description");
			string speechBubbleDescription = Localizer.Localize(eventTranslationIdPrefix + ".SpeechBubble");

			Title.text = titleText;
			Description.text = eventDescription;
			Timer.Setup(model.ActivableParams);

			var content = diContainer.InstantiatePrefabForComponent<DiscountEventAdvertisementContent>(
				PrefabDiscountEventAdvertisementContent,
				EventContent
			);
			content.Setup((DiscountEventInfo)model.EventInfo);
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await UniTask.WhenAny(
				OkButton.OnClickAsync(ct),
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			if (CloseButton != null) {
				CloseButton.onClick.Invoke();
			}
		}
	}
}
