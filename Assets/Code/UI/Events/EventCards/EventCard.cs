using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Events.EventCards {
	public class EventCard : ButtonHelper {
		[SerializeField] private GameObject ActiveContainer;
		[SerializeField] private GameObject PreviewContainer;

		[Inject] private IEventsFlowController eventsFlowController;

		public IEventModel EventModel { private set; get; }

		public virtual void Setup(IEventModel eventModel) {
			EventModel = eventModel;

			bool isActive =
				eventModel.Status(MetaplayClient.PlayerModel) is MetaActivableVisibleStatus.Active ||
				eventModel.Status(MetaplayClient.PlayerModel) is MetaActivableVisibleStatus.EndingSoon;

			ActiveContainer.SetActive(isActive);
			PreviewContainer.SetActive(!isActive);
		}

		protected override void OnClick() {
			eventsFlowController.ShowEventUI(
				EventModel,
				gameObject.GetCancellationTokenOnDestroy()
			);
		}
	}
}
