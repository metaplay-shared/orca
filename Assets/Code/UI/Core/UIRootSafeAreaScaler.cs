using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI.Core {
	/// <summary>
	/// This unity component is designed to be added on the part of an UI root that should be affected by the
	/// devices screen safe area and resolution. It will adjust and scale the boundaries of its RectTransform
	/// to ensure the content to always fill the screen nicely. The CanvasScaler should be setup to fit the
	/// width of the screen.
	/// </summary>
	[ExecuteAlways]
	public class UIRootSafeAreaScaler : UIBehaviour, ILayoutIgnorer {
		[Header("Big Screen Adjustments")]
		[Tooltip("Define at which screen ration adjustments start to take effect. " +
			"A value of 1 means a perfectly squared screen, while 2 means a 1:2 portrait screen.")]
		[Range(min: 1f, max: 2f)]
		[SerializeField] private float MinScreenRatio = 1f;
		[Tooltip("Define at which screen width in pixels the screen adjustments get triggered. " +
			"On screens in portrait mode with a horizontal resolution higher than this number, " +
			"the popup will get scaled down to not fill the entire screen.")]
		[SerializeField] private int TriggerScreenWidth = 1200;
		[Header("Ignore Flags")]
		[SerializeField] private bool IgnoreLeft;
		[SerializeField] private bool IgnoreRight;
		[SerializeField] private bool IgnoreTop;
		[SerializeField] private bool IgnoreBottom;
		
		private DrivenRectTransformTracker tracker;
		
		private RectTransform RectTransform => GetComponent<RectTransform>();
		private Canvas Canvas => GetComponentInParent<Canvas>();

		protected override void OnEnable() {
			base.OnEnable();
			Canvas.willRenderCanvases += UpdateRect;
		}

		protected override void OnDisable() {
			base.OnDisable();
			Canvas.willRenderCanvases -= UpdateRect;
			tracker.Clear();
		}

		#if UNITY_EDITOR
		protected override void OnValidate() {
			base.OnValidate();
			if (!EditorApplication.isPlayingOrWillChangePlaymode) {
				UpdateRect();
			}
		}
		#endif

		private void UpdateRect() {
			if (Canvas == null) {
				return;
			}
			
			var safeAreaWidth = Screen.safeArea.width;
			var safeAreaHeight = Screen.safeArea.height;
			var safeAreaPosition = Screen.safeArea.position;
			var screenWidth = Screen.width;
			var screenHeight = Screen.height;
			var scaleFactor = Canvas.scaleFactor;

			if (IgnoreLeft) {
				safeAreaWidth += safeAreaPosition.x;
				safeAreaPosition.x = 0;
			}
			if (IgnoreRight) {
				safeAreaWidth += screenWidth - safeAreaWidth - safeAreaPosition.x;
			}
			if (IgnoreTop) {
				safeAreaHeight += screenHeight - safeAreaHeight - safeAreaPosition.y;
			}
			if (IgnoreBottom) {
				safeAreaHeight += safeAreaPosition.y;
				safeAreaPosition.y = 0;
			}

			var drivenProperties = DrivenTransformProperties.None;

			RectTransform.anchorMin = Vector2.zero;
			RectTransform.anchorMax = Vector2.one;
			drivenProperties |= DrivenTransformProperties.Anchors;

			RectTransform.sizeDelta = new Vector2(
				safeAreaWidth - screenWidth,
				safeAreaHeight - screenHeight
			);

			drivenProperties |= DrivenTransformProperties.SizeDelta;

			RectTransform.pivot = Vector2.zero;
			drivenProperties |= DrivenTransformProperties.Pivot;

			RectTransform.anchoredPosition = safeAreaPosition / scaleFactor;
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, safeAreaWidth / scaleFactor);
			RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, safeAreaHeight / scaleFactor);
			drivenProperties |= DrivenTransformProperties.AnchoredPosition;

			RectTransform.localRotation = Quaternion.identity;
			drivenProperties |= DrivenTransformProperties.Rotation;

			RectTransform.localScale = Vector3.one;
			drivenProperties |= DrivenTransformProperties.Scale;
			
			var screenRatio = (float) screenHeight / screenWidth;
			var isBigScreenAdjustmentRequired =
				screenRatio < MinScreenRatio &&
				screenWidth >= TriggerScreenWidth;

			if (isBigScreenAdjustmentRequired) {
				
				var preferredRatio = Mathf.Max(screenRatio, MinScreenRatio);
				var ratioDecreaseFactor = screenRatio / preferredRatio;

				// Adjust scale to simulate a higher screen resolution
				RectTransform.localScale *= ratioDecreaseFactor;
				
				// Push the rect boarders up again to always fill the screen vertically
				var offsetMin = RectTransform.offsetMin;
				var offsetMax = RectTransform.offsetMax;
				var anchoredPosition = RectTransform.anchoredPosition;

				var scaledScreenHeight = screenHeight / scaleFactor;
				var scaledScreenWidth = screenWidth / scaleFactor;
				
				var lostHeight = scaledScreenHeight / ratioDecreaseFactor - scaledScreenHeight;
				var lostWidth = scaledScreenWidth / ratioDecreaseFactor - scaledScreenWidth;

				anchoredPosition.x += lostWidth * ratioDecreaseFactor / 2;

				offsetMin.y -= lostHeight / 2;
				offsetMax.y += lostHeight / 2;

				// Unity sometimes returns 0 sizes in editor when switching resolutions.
				// Guard against that by only applying valid numbers to the rect transform.
				if (!float.IsNaN(offsetMin.x) &&
					!float.IsNaN(offsetMin.y) &&
					!float.IsInfinity(offsetMin.x) &&
					!float.IsInfinity(offsetMin.y)
				) {
					RectTransform.offsetMin = offsetMin;
				}
				
				if (!float.IsNaN(offsetMax.x) &&
					!float.IsNaN(offsetMax.y) &&
					!float.IsInfinity(offsetMax.x) &&
					!float.IsInfinity(offsetMax.y)
				) {
					RectTransform.offsetMax = offsetMax;
				}
				
				if (!float.IsNaN(anchoredPosition.x) &&
					!float.IsNaN(anchoredPosition.y) &&
					!float.IsInfinity(anchoredPosition.x) &&
					!float.IsInfinity(anchoredPosition.y)
				) {
					RectTransform.anchoredPosition = anchoredPosition;
				}
			}

			tracker.Add(this, RectTransform, drivenProperties);

			LayoutRebuilder.MarkLayoutForRebuild(RectTransform);
		}

		public bool ignoreLayout => false;
	}
}