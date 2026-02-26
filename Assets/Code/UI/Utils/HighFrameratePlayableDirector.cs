using UnityEngine;
using UnityEngine.Playables;

namespace Code.UI.Utils {
	[RequireComponent(typeof(PlayableDirector))]
	public class HighFrameratePlayableDirector : HighFramerateBehaviourBase {
		private PlayableDirector playableDirector;

		protected void Awake() {
			playableDirector = GetComponent<PlayableDirector>();
		}

		protected override bool IsMoving() => playableDirector.state == PlayState.Playing;
	}
}
