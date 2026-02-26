using Code.UI.Application;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Zenject;

namespace Code.UI.CloudCurtains {
	public interface ICloudCurtainsPresenter {
		UniTask Close(CancellationToken ct);
		UniTask Open(CancellationToken ct);
	}

	public class CloudCurtainsPresenter : MonoBehaviour, ICloudCurtainsPresenter, IInitializable {
		[SerializeField] private PlayableDirector PlayableDirector;
		[SerializeField] private TimelineAsset CloseTimeline;
		[SerializeField] private TimelineAsset OpenTimeline;

		[Inject] private IFrameRateController frameRateController;

		public void Initialize() {
			gameObject.SetActive(false);
		}

		public async UniTask Close(CancellationToken ct) {
			using IDisposable frameControl = frameRateController.RequestHighFPS();
			gameObject.SetActive(true);
			await PlayableDirector.PlayAsync(CloseTimeline, cancellationToken: ct);
		}

		public async UniTask Open(CancellationToken ct) {
			using IDisposable frameControl = frameRateController.RequestHighFPS();
			await PlayableDirector.PlayAsync(OpenTimeline, cancellationToken: ct);
			gameObject.SetActive(false);
		}
	}
}