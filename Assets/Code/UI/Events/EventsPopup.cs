using System.Threading;
using Code.UI.Core;
using Code.UI.Events.EventCards;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class EventsPopupUIHandle : UIHandleBase { }

	public class EventsPopup : UIRootBase<EventsPopupUIHandle> {
		[SerializeField] private RectTransform EventsContainer;
		[SerializeField] private DiscountEventCard PrefabDiscountEventCard;
		[SerializeField] private ActivityEventCard PrefabActivityEventCard;
		[SerializeField] private DailyTasksEventCard PrefabDailyTasksEventCard;

		[SerializeField] private Button CloseButton;

		[Inject] private DiContainer container;

		protected override void Init() {
			Clear();
			SetupCards();
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		private void Clear() {
			foreach (Transform child in EventsContainer) {
				Destroy(child.gameObject);
			}
		}

		private void SetupCards() {
			// Add daily task card

			foreach (var eventModel in MetaplayClient.PlayerModel.VisibleEventModels()) {
				GameObject eventCard = EventCardForModel(eventModel);
				eventCard.transform.SetParent(EventsContainer, false);
			}
		}

		private GameObject EventCardForModel(IEventModel eventModel) {
			if (eventModel is DiscountEventModel discountEventModel) {
				DiscountEventCard discountEventCard =
					container.InstantiatePrefabForComponent<DiscountEventCard>(PrefabDiscountEventCard);
				discountEventCard.Setup(discountEventModel);

				return discountEventCard.gameObject;
			}

			if (eventModel is ActivityEventModel activityEventModel) {
				ActivityEventCard activityEventCard =
					container.InstantiatePrefabForComponent<ActivityEventCard>(PrefabActivityEventCard);
				activityEventCard.Setup(activityEventModel);

				return activityEventCard.gameObject;
			}

			if (eventModel is DailyTaskEventModel dailyTaskEventModel) {
				DailyTasksEventCard dailyTasksEventCard =
					container.InstantiatePrefabForComponent<DailyTasksEventCard>(PrefabDailyTasksEventCard);
				dailyTasksEventCard.Setup(dailyTaskEventModel);

				return dailyTasksEventCard.gameObject;
			}

			return null;
		}
	}
}
