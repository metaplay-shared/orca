using Code.UI.Core;
using Code.UI.Events.DiscountEvent;
using Code.UI.Events.SeasonalEvent;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using System.Threading;
using UnityEngine;

namespace Code.UI.Events {
	public interface IEventsFlowController {
		UniTask ShowEventUI(IEventModel eventModel, CancellationToken ct);
		UniTask ShowStartupAdvertisements(CancellationToken ct);
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class EventsFlowController : IEventsFlowController {
		private readonly IUIRootController uiRootController;

		public EventsFlowController(
			IUIRootController uiRootController
		) {
			this.uiRootController = uiRootController;
		}

		public async UniTask ShowEventUI(IEventModel eventModel, CancellationToken ct) {
			UniTask GetShowTask() {
				if (eventModel.Status(MetaplayClient.PlayerModel) is MetaActivableVisibleStatus.InPreview) {
					return uiRootController.ShowUI<EventAdvertisementPopup, EventAdvertisementPopupPayload>(
						new EventAdvertisementPopupPayload(eventModel),
						ct
					).OnComplete;
				}

				switch (eventModel) {
					case ActivityEventModel activityEventModel:
						return uiRootController.ShowUI<ActivityEventDetailsPopup, EventAdvertisementPopupPayload>(
							new EventAdvertisementPopupPayload(activityEventModel),
							ct
						).OnComplete;
					case DailyTaskEventModel dailyTaskEventModel:
						return uiRootController.ShowUI<DailyTasksUIRoot, DailyTasksUIHandle>(
							new DailyTasksUIHandle(dailyTaskEventModel),
							ct
						).OnComplete;
					case DiscountEventModel discountEventModel:
						return uiRootController.ShowUI<DiscountEventUIRoot, DiscountEventUIHandle>(
							new DiscountEventUIHandle(discountEventModel),
							ct
						).OnComplete;
					case SeasonalEventModel seasonalEventModel:
						return uiRootController.ShowUI<SeasonalEventUIRoot, SeasonEventUIRootHandle>(
							new SeasonEventUIRootHandle(seasonalEventModel),
							ct
						).OnComplete;
					default:
						Debug.LogError(
							$"No implementation for handling event button interaction with type: {eventModel.GetType()}"
						);
						break;
				}

				return UniTask.CompletedTask;
			}

			await GetShowTask();

			MetaplayClient.PlayerContext.ExecuteAction(
				new PlayerViewActivityEventAd(eventModel.EventInfo.ActivableId)
			);
		}

		public async UniTask ShowStartupAdvertisements(CancellationToken ct) {
			foreach (IEventModel eventModel in MetaplayClient.PlayerModel.VisibleEventModels()) {
				if (ShouldShowAdvertisement(eventModel)) {
					await ShowEventUI(eventModel, ct);
				}
			}
		}

		private bool ShouldShowAdvertisement(IEventModel eventModel) {
			if (eventModel.AdSeen) {
				return false;
			}

			if (eventModel.AdMode == EventAdMode.None) {
				return false;
			}

			MetaActivableVisibleStatus status = eventModel.Status(MetaplayClient.PlayerModel);

			if (eventModel.AdMode == EventAdMode.Always &&
				status
					is MetaActivableVisibleStatus.InPreview
					or MetaActivableVisibleStatus.Active
					or MetaActivableVisibleStatus.EndingSoon
				) {
				return true;
			}

			if (eventModel.AdMode == EventAdMode.OnPreview &&
				status is MetaActivableVisibleStatus.InPreview) {
				return true;
			}

			if (eventModel.AdMode == EventAdMode.OnActive &&
				status
					is MetaActivableVisibleStatus.Active
					or MetaActivableVisibleStatus.EndingSoon) {
				return true;
			}

			return false;
		}
	}
}
