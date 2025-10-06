using Code.UI.Application;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Metaplay.Unity.DefaultIntegration;
using System;
using UnityEngine;
using Zenject;

namespace Code.UI.HudBase {
	public class FlightTarget : MonoBehaviour {
		public bool CanFlyHere => gameObject.activeInHierarchy;

		[Inject] private SignalBus signalBus;
		[Inject] private IFrameRateController frameRateController;

		private Transform topLayer;

		private void Start() {
			topLayer = GameObject.Find("TopLayer").transform;
		}

		public async UniTask FlyFromAsync(RectTransform targetObject) {
			using IDisposable frameRateHandle = frameRateController.RequestHighFPS();
			targetObject.SetParent(topLayer, true);
			await CreateFlightAnimation(targetObject);
			signalBus.Fire(new ItemFlightCompletedSignal());
			Destroy(targetObject.gameObject);
		}

		protected virtual Sequence CreateFlightAnimation(RectTransform targetObject) {
			Vector3 position = transform.position;
			float distance = Vector3.Distance(position, targetObject.position);
			float duration = MetaplayClient.PlayerModel.GameConfig.Client.GetItemFlightTime(distance);

			return DOTween.Sequence()
				.Join(
					targetObject.DOMove(position, duration).SetEase(Ease.OutQuad)
				).Join(
					targetObject.DOScale(Vector3.one * 2f, duration / 2)
				).Insert(
					duration / 2,
					targetObject.DOScale(Vector3.one, duration / 2)
				);
		}
	}
}
