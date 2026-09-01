using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Tutorial.TutorialPointer {
	public class PointerAnimation : MonoBehaviour {
		private const float FRAME_TIME = 0.1f;

		[SerializeField] private Image ImageToAnimate;
		[SerializeField] private Sprite[] Frames;

		private int animationStart = 0;
		private int animationEnd = 0;
		private int currentFrame = 0;
		private float frameTime = 0.0f;
		private bool isPlaying;
		private bool isLooping;

		public void Loop() {
			PlayRange(0, Frames.Length - 1, true);
		}

		public void PlayRange(int begin, int end, bool loop) {
			if (begin > end) {
				throw new ArgumentException("Begin cannot be larger than end");
			}

			if (begin >= Frames.Length ||
				end >= Frames.Length) {
				throw new ArgumentOutOfRangeException();
			}

			isPlaying = true;
			animationStart = begin;
			animationEnd = end;
			isLooping = loop;
		}

		public void ResetAndStop() {
			isPlaying = false;
			isLooping = false;
			currentFrame = 0;
			SetFrame(0);
		}

		private void Update() {
			if (!isPlaying) {
				return;
			}

			frameTime += Time.deltaTime;
			if (frameTime >= FRAME_TIME) {
				frameTime = 0;

				currentFrame++;

				if (currentFrame > animationEnd) {
					currentFrame = animationStart;
				}

				SetFrame(currentFrame);
			}
		}

		private void SetFrame(int frameNumber) {
			ImageToAnimate.sprite = Frames[frameNumber];
		}

		public Tween TweenPlayRange(int start, int end) {
			isPlaying = false;
			isLooping = false;

			float duration = (end - start) * FRAME_TIME;

			currentFrame = start;
			var tween = DOTween.To(
				() => currentFrame,
				(frame) => {
					currentFrame = frame;
					SetFrame(currentFrame);
				},
				end,
				duration
			);

			tween.startValue = start;

			return tween;
		}
	}
}
