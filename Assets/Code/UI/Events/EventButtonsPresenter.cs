using System.Collections.Generic;
using System.Linq;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Events {
	public class EventButtonsPresenter : MonoBehaviour {
		/// <summary>
		/// Prefab used for visualizing active events
		/// </summary>
		[SerializeField] private EventButton DiscountEventButtonPrefab;
		[SerializeField] private EventButton ActivityEventButtonPrefab;
		[SerializeField] private EventButton DailyTaskEventButtonPrefab;
		[SerializeField] private EventButton SeasonalEventButtonPrefab;
		/// <summary>
		/// Transform in hierarchy in which the EventButtons will be instantiated
		/// </summary>
		[SerializeField] private RectTransform EventButtonsContainer;

		[Inject] private SignalBus signalBus;
		[Inject] private DiContainer diContainer;
		[Inject] private IEventsFlowController eventsFlowController;

		private List<EventButton> eventButtons = new();

		private PlayerModel PlayerModel => MetaplayClient.PlayerModel;

		private void OnEnable() {
			signalBus.Subscribe<EventStateChangedSignal>(EventStateChanged);
			UpdateEvents();
		}

		private void OnDisable() {
			signalBus.TryUnsubscribe<EventStateChangedSignal>(EventStateChanged);
		}

		private void UpdateEvents() {
			IEnumerable<IEventModel> activeEvents = PlayerModel.VisibleEventModels().Where(
				evt => {
					MetaActivableVisibleStatus metaActivableVisibleStatus = evt.Status(PlayerModel);
					return metaActivableVisibleStatus is MetaActivableVisibleStatus.InPreview
						or MetaActivableVisibleStatus.Active
						or MetaActivableVisibleStatus.EndingSoon;
				}
			).Where(evt => evt is not DailyTaskEventModel);

			foreach (IEventModel activeEvent in activeEvents) {
				foreach (EventButton eventButton in eventButtons) {
					// TODO: Null checking should not be necessary here, but since setup of the event button is
					//		 deferred from construction, the EventModel might not be set. In that case we just re-create
					//		 the button for now
					if (eventButton.EventModel == null ||
						eventButton.EventModel.EventId == activeEvent.EventId) {
						Destroy(eventButton.gameObject);
					}
				}

				eventButtons.RemoveAll(
					button => button.EventModel == null || button.EventModel.EventId == activeEvent.EventId
				);
			}

			foreach (IEventModel activeEvent in activeEvents) {
				eventButtons.Add(CreateEventButton(activeEvent));
			}

			SortEventButtons();
		}

		private void SortEventButtons() {
			var orderedButtons = eventButtons.OrderBy(btn => btn.EventModel.VisualizationOrder);
			int i = 0;
			foreach (EventButton eventButton in orderedButtons) {
				eventButton.transform.SetSiblingIndex(i++);
			}
		}

		private void EventStateChanged(EventStateChangedSignal arg) {
			UpdateEvents();
		}

		private EventButton CreateEventButton(IEventModel eventModel) {
			EventButton eventButtonPrefab = GetPrefabForEventModel(eventModel);
			EventButton eventButton =
				diContainer.InstantiatePrefabForComponent<EventButton>(eventButtonPrefab, EventButtonsContainer);
			eventButton.SetupEventModel(eventModel, OnEventButtonClicked);
			return eventButton;
		}

		private void OnEventButtonClicked(EventButton eventButton) {
			eventsFlowController.ShowEventUI(eventButton.EventModel, gameObject.GetCancellationTokenOnDestroy()).Forget();
		}

		[CanBeNull]
		private EventButton GetPrefabForEventModel(IEventModel eventModel) {
			return eventModel switch {
				DiscountEventModel discountEventModel   => DiscountEventButtonPrefab,
				ActivityEventModel activityEventModel   => ActivityEventButtonPrefab,
				DailyTaskEventModel dailyTaskEventModel => DailyTaskEventButtonPrefab,
				SeasonalEventModel seasonalEventModel   => SeasonalEventButtonPrefab,
				_                                       => null
			};
		}
	}
}
