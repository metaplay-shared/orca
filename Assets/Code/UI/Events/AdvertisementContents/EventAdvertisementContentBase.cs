using UnityEngine;

namespace Code.UI.Events.AdvertisementContents {
	public abstract class EventAdvertisementContentBase<TContentInfo> : MonoBehaviour {
		[SerializeField] private RectTransform PreviewContainer;
		[SerializeField] private RectTransform ActiveContainer;

		public void Setup(TContentInfo info) {
			PreviewContainer.gameObject.SetActive(false);
			ActiveContainer.gameObject.SetActive(true);
			Setup(info, true);
		}
		protected abstract void Setup(TContentInfo info, bool active);
	}
}
