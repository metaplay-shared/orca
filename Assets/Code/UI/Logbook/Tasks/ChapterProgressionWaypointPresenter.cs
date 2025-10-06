using UnityEngine;

namespace Code.UI.Logbook.Tasks {
	public class ChapterProgressionWaypointPresenter : MonoBehaviour {
		[SerializeField] private GameObject CompletedIcon;

		public void SetActive(bool active) {
			CompletedIcon.SetActive(active);
		}
	}
}
