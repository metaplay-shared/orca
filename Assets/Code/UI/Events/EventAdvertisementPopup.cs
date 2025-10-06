using System.Threading;
using Code.UI.Core;
using Code.UI.Events.AdvertisementContents;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.Activables;
using Metaplay.Core.Player;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class EventAdvertisementPopupPayload : UIHandleBase {
		public IEventModel EventModel { get; }

		public EventAdvertisementPopupPayload(IEventModel eventModel) {
			EventModel = eventModel;
		}
	}

	public class EventAdvertisementPopup : UIRootBase<EventAdvertisementPopupPayload> {
		[SerializeField] private TMP_Text HeaderText;
		[SerializeField] private TMP_Text StartTimeText;
		[SerializeField] private TMP_Text EndTimeText;
		[SerializeField] private RectTransform EventContent;

		[SerializeField] protected Button OkButton;
		[SerializeField] protected Button CloseButton;

		[Header("Prefabs")]
		[SerializeField] private DiscountEventAdvertisementContent PrefabDiscountEventAdvertisementContent;
		[SerializeField] private ActivityEventAdvertisementContent PrefabActivityEventAdvertisementContent;

		[Inject] private DiContainer diContainer;

		protected override void Init() {
			SetupContent();
		}

		private void SetupContent() {
			if (UIHandle.EventModel.EventInfo is DiscountEventInfo discountInfo) {
				DiscountEventAdvertisementContent content =
					diContainer.InstantiatePrefabForComponent<DiscountEventAdvertisementContent>(
						PrefabDiscountEventAdvertisementContent,
						EventContent
					);
				content.Setup(discountInfo);
			} else if (UIHandle.EventModel.EventInfo is ActivityEventInfo activityEventInfo) {
				ActivityEventAdvertisementContent content =
					diContainer.InstantiatePrefabForComponent<ActivityEventAdvertisementContent>(
						PrefabActivityEventAdvertisementContent,
						EventContent
					);
				content.Setup(activityEventInfo);
			} else {
				Debug.LogError("Unknown event type: " + UIHandle.EventModel.EventId.Value);
			}
		}

		private void Update() {
			if (UIHandle.EventModel.Status(MetaplayClient.PlayerModel) is MetaActivableVisibleStatus.InPreview) {
				HeaderText.text = "Upcoming event!";
			}

			if (UIHandle.EventModel.Status(MetaplayClient.PlayerModel) is MetaActivableVisibleStatus.Active) {
				HeaderText.text = "Ongoing event";
				UpdateEndingTime();
				return;
			}

			UpdateStartingTime();
		}

		private void UpdateStartingTime() {
			var currentOrNextEnabledOccasion = UIHandle.EventModel.EventInfo.ActivableParams.Schedule
				.QueryOccasions(MetaplayClient.PlayerModel.GetCurrentLocalTime()).CurrentOrNextEnabledOccasion;

			if (currentOrNextEnabledOccasion == null) {
				return;
			}

			var startTime = currentOrNextEnabledOccasion.Value
				.EnabledRange.Start;
			StartTimeText.text =
				$"Starts in {(startTime - MetaplayClient.PlayerModel.CurrentTime).ToSimplifiedString()}";
		}

		private void UpdateEndingTime() {
			var currentOrNextEnabledOccasion = UIHandle.EventModel.EventInfo.ActivableParams.Schedule
				.QueryOccasions(MetaplayClient.PlayerModel.GetCurrentLocalTime()).CurrentOrNextEnabledOccasion;

			if (currentOrNextEnabledOccasion == null) {
				return;
			}

			var endTime = currentOrNextEnabledOccasion.Value
				.EnabledRange.End;
			StartTimeText.text =
				$"Ends in {(endTime - MetaplayClient.PlayerModel.CurrentTime).ToSimplifiedString()}";
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
