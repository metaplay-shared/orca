using System.Threading;
using Code.UI.Core;
using Game.Logic;
using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Events.EventCards {
	public class ActivityEventCard : EventCard {
		[SerializeField] private TMP_Text ScoreText;

		[Inject] private IUIRootController uiRootController;

		public override void Setup(IEventModel eventModel) {
			base.Setup(eventModel);

			ActivityEventModel activityEventModel = eventModel as ActivityEventModel;
			ScoreText.text = activityEventModel.EventLevel.CurrentXp.ToString();
		}

		protected override void OnClick() {
			bool isActive = !(EventModel.Status(MetaplayClient.PlayerModel) is MetaActivableVisibleStatus.InPreview);
			if (isActive) {
				uiRootController.ShowUI<ActivityEventDetailsPopup, EventAdvertisementPopupPayload>(new EventAdvertisementPopupPayload(EventModel), CancellationToken.None);
			} else {
				base.OnClick();
			}
		}
	}
}
