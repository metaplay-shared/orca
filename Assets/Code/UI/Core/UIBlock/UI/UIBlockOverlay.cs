using UnityEngine;

namespace Code.UI.Core.UIBlock.UI {
	public interface IUIBlockOverlay {
		void DontDestroyOnLoad();
		void SetState(UIBlockState state);
		void Destroy();
	}
	
	public class UIBlockOverlay : MonoBehaviour, IUIBlockOverlay {

		[SerializeField] private CanvasGroup OverlayCanvasGroup;

		public void Awake() => gameObject.SetActive(false);

		public void DontDestroyOnLoad() => DontDestroyOnLoad(gameObject);

		public void SetState(UIBlockState state) {
			// TODO: Add a more eye pleasing transition between the states
			gameObject.SetActive(state != UIBlockState.Unblocked);
			OverlayCanvasGroup.alpha = state == UIBlockState.Overlay ? 1 : 0;
		}

		public void Destroy() => Destroy(gameObject);
	}
}
