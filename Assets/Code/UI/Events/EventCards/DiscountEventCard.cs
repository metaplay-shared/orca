using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.Core;
using Code.UI.Events.DiscountEvent;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events.EventCards {
	public class DiscountEventCard : EventCard {
		[Header("Preview section")]
		[SerializeField] private TMP_Text PreviewTitle;

		[Header("Active section")]
		[SerializeField] private TMP_Text ActiveTitle;
		[SerializeField] private TMP_Text ActiveDescription;

		[SerializeField] private Image Icon;

		[Inject] private IUIRootController uiRootController;
		[Inject] private AddressableManager addressableManager;

		private PlayerModel PlayerModel => MetaplayClient.PlayerModel;

		private DiscountEventInfo eventInfo;

		public override void Setup(IEventModel eventModel) {
			base.Setup(eventModel);
			eventInfo = (DiscountEventInfo)EventModel.EventInfo;

			SetupIcons(eventInfo, this.GetCancellationTokenOnDestroy()).Forget();
			SetupTexts();
		}

		private async UniTask SetupIcons(DiscountEventInfo discountEventInfo, CancellationToken ct) {
			Icon.sprite = await addressableManager.GetLazy<Sprite>(discountEventInfo.Icon)
				.AttachExternalCancellation(ct);
		}

		private void SetupTexts() {
			const string EVENT_INFO_LOCALE_PREFIX = "Event.Info.Discount.";
			DiscountEventType eventInfoDiscountEventType = eventInfo.DiscountEventType;

			string eventName = Localizer.Localize(EVENT_INFO_LOCALE_PREFIX + eventInfoDiscountEventType + ".Title");

			PreviewTitle.text = Localizer.Localize("Event.Info.StartingSoon", eventName);
			ActiveTitle.text = eventName;

			if (eventInfoDiscountEventType == DiscountEventType.Energy) {
				int percentage = (int)((eventInfo.EnergyProductionFactor.Float - 1.0f) * 100);
				ActiveDescription.text = Localizer.Localize("Event.Info.Discount.Energy", percentage);
			} else if (eventInfoDiscountEventType == DiscountEventType.BuilderTimer) {
				int percentage = (int)((1.0 - eventInfo.BuilderTimerFactor.Float) * 100);
				ActiveDescription.text = Localizer.Localize("Event.Info.Discount.BuilderTimer", percentage);
			} else if (eventInfoDiscountEventType == DiscountEventType.BuilderTimerGold) {
				ActiveDescription.text = Localizer.Localize("Event.Info.Discount.BuilderTimerGold");
			}
		}

		protected override void OnClick() {
			if (PlayerModel.DiscountEvents.TryGetVisibleStatus(
					eventInfo,
					PlayerModel,
					out MetaActivableVisibleStatus visibleStatus
				)) {
				switch (visibleStatus) {
					case MetaActivableVisibleStatus.InPreview inPreview:
						base.OnClick();
						break;
					default:
						ShowUI();
						break;
				}
			}
		}

		private void ShowUI() {
			uiRootController.ShowUI<DiscountEventUIRoot, DiscountEventUIHandle>(
				new DiscountEventUIHandle(EventModel),
				this.GetCancellationTokenOnDestroy()
			);
		}
	}
}
