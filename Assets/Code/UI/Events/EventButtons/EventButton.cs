using System;
using System.Threading;
using Code.UI.AssetManagement;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Core.Activables;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events {
	public class EventButton : MonoBehaviour {
		[Header("Preview state")]
		[SerializeField] private GameObject PreviewGameObject;
		[SerializeField] private Image PreviewEventIcon;
		[SerializeField] private EventTimer PreviewTimer;

		[Header("Active state")]
		[SerializeField] private GameObject ActiveGameObject;
		[SerializeField] private Image EventIcon;
		[SerializeField] private EventTimer Timer;

		[SerializeField] private Button Button;

		[Inject] private AddressableManager addressableManager;
		private Action<EventButton> eventButtonClickCallback;

		public IEventModel EventModel { get; protected set; }
		private PlayerModel PlayerModel => MetaplayClient.PlayerModel;

		public virtual void SetupEventModel(IEventModel eventModel, Action<EventButton> eventButtonClickCallback) {
			MetaActivableVisibleStatus metaActivableVisibleStatus = eventModel.Status(PlayerModel);

			PreviewGameObject.SetActive(metaActivableVisibleStatus is MetaActivableVisibleStatus.InPreview);
			ActiveGameObject.SetActive(
				metaActivableVisibleStatus is MetaActivableVisibleStatus.Active or MetaActivableVisibleStatus.EndingSoon
			);

			this.EventModel = eventModel;

			PreviewTimer.Setup(eventModel.MetaActivableParams);
			Timer.Setup(eventModel.MetaActivableParams);

			LoadAndSetIconAsync(eventModel.Icon, this.GetCancellationTokenOnDestroy()).Forget();

			this.eventButtonClickCallback = eventButtonClickCallback;
			Button.onClick.RemoveAllListeners();
			Button.onClick.AddListener(() => this.eventButtonClickCallback?.Invoke(this));
		}

		private async UniTask LoadAndSetIconAsync(string addressablePath, CancellationToken ct) {
			Sprite sprite = await addressableManager.GetLazy<Sprite>(addressablePath).AttachExternalCancellation(ct);
			EventIcon.sprite = sprite;
			PreviewEventIcon.sprite = sprite;
		}
	}
}
