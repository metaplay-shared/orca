using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Code.UI.Utils {
	public static class PlayableDirectorExtensions {
		public static async UniTask PlayAsync(
			this PlayableDirector target,
			TimelineAsset timeline,
			CancellationToken cancellationToken = default
		) {
			using (cancellationToken.Register(() => target.Pause())) {
				target.Play(timeline);
				await UniTask.WaitWhile(() => target.state == PlayState.Playing, cancellationToken: cancellationToken);
			}
		}
	}
}
