using Code.UI.HudBase;
using DG.Tweening;
using Orca.Unity.Utilities;
using UnityEngine;

namespace Code.UI.Merge {
	public class NavigationButtonFlightTarget : FlightTarget {
		[SerializeField] private RectTransform Icon;
		[SerializeField] private float FlightSpeed = 4f;
		[SerializeField] private float IconJumpHeight = 25f;
		[SerializeField] private Vector3 IconPunchScale = new Vector3(-0.1f, 0.1f);
		[SerializeField] private float IconAnimationDuration = 0.35f;

		private void OnHit() {
			DOTween.Complete(Icon);
			Icon.DOPunchAnchorPos(Vector2.up * IconJumpHeight, IconAnimationDuration, 0);
			Icon.DOPunchScale(IconPunchScale, IconAnimationDuration, 0);
		}

		protected override Sequence CreateFlightAnimation(RectTransform targetObject) {
			Vector3 position = transform.position;
			float distance = Vector3.Distance(position, targetObject.position);
			float duration = distance / FlightSpeed;
			return DOTween.Sequence()
				.Join(
					targetObject.DOMoveX(position.x, duration).SetEase(Ease.OutQuad)
				).Join(
					targetObject.DOMoveY(position.y, duration).SetEase(Ease.InQuad)
				).Join(
					targetObject.DOScale(Vector3.one * 1.5f, duration / 2)
				).Insert(
					duration / 2,
					targetObject.DOScale(Vector3.one * 0.75f, duration / 2)
				).Insert(
					duration / 6 * 5,
					targetObject.GetOrAddComponent<CanvasGroup>().DOFade(0f, duration / 6)
				).InsertCallback(
					duration / 6 * 5,
					OnHit
				);
		}
	}
}
