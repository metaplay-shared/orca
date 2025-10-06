using System;
using System.Threading.Tasks;
using Code.UI.Application;
using Code.UI.MergeBase;
using Code.UI.MergeBase.Signals;
using Code.UI.Tutorial;
using Code.UI.Tutorial.TutorialPointer;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Game.Logic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI {
	public class Blackout : MonoBehaviour {
		private const float FADE_TIME = 0.25f;

		[Inject] private SignalBus signalBus;
		[Inject] private MergeBoardRoot mergeBoard;
		[Inject] private PointerRoot tutorialPointer;
		[Inject] private DiContainer container;

		private Image image;

		private Color activeColor;
		private int fadeCounter;
		private TweenerCore<Color, Color, ColorOptions> tween;
		public Button.ButtonClickedEvent OnClick => GetComponent<Button>().onClick;

		private void Awake() {
			image = GetComponent<Image>();
			image.enabled = false;
			activeColor = image.color;
		}

		private void OnEnable() {
			signalBus.Subscribe<MergeHintSignal>(OnMergeHintSignal);
			signalBus.Subscribe<HighlightElementSignal>(OnHighlightElement);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<MergeHintSignal>(OnMergeHintSignal);
			signalBus.Unsubscribe<HighlightElementSignal>(OnHighlightElement);
		}

		public async Task HighlightItem(ItemModel model) {
			await UniTask.DelayFrame(2);

			using (var highlight = container.Resolve<ItemHighlight>()) {
				var item = mergeBoard.ItemAt(model.X, model.Y);
				if (item == null) {
					return;
				}
				var tile = mergeBoard.TileAt(model.X, model.Y).Handle;

				tutorialPointer.Point(tile.transform.position);
				await highlight.Run(tile, item.Handle);
				tutorialPointer.Hide();
			}
		}

		private async void OnMergeHintSignal(MergeHintSignal signal) {
			await MergeHint(signal.ItemModelA, signal.ItemModelB);
		}

		public async UniTask MergeHint(ItemModel itemModelA, ItemModel itemModelB) {
			await UniTask.DelayFrame(2);

			if (itemModelA == null ||
				itemModelB == null) {
				Debug.LogWarning(
					$"Trying to show MergeHint while targets were null: Targets: A: {itemModelA?.Info.ConfigKey} B: {itemModelB?.Info.ConfigKey}"
				);
				return;
			}

			using (var highlight = container.Resolve<ItemMergeHighlight>()) {
				var itemA = mergeBoard.ItemAt(itemModelA.X, itemModelA.Y);
				var tileA = mergeBoard.TileAt(itemModelA.X, itemModelA.Y);
				var itemB = mergeBoard.ItemAt(itemModelB.X, itemModelB.Y);
				var tileB = mergeBoard.TileAt(itemModelB.X, itemModelB.Y);

				itemA.StopAnimations();
				itemB.StopAnimations();

				tutorialPointer.Swipe(tileA.transform.position, tileB.transform.position);

				await highlight.Run(tileA.Handle, tileB.Handle, itemA.Handle, itemB.Handle);
				tutorialPointer.Hide();
			}
		}

		private async void OnHighlightElement(HighlightElementSignal signal) {
			await HighlightElement(signal.Type);
		}

		public async Task HighlightElement(string type) {
			await UniTask.DelayFrame(2);

			GameObject highlightGo = HighlightableElement.FindGameObjectWithType(type);
			if (highlightGo == null) {
				return;
			}

			using (var highlight = container.Resolve<UiElementHighlight>()) {
				await highlight.Run(highlightGo);
			}
		}

		public async UniTask FadeOut() {
			fadeCounter--;

			if (fadeCounter > 0) {
				//return;
			}

			if (tween != null) {
				tween.Kill();
			}

			tween = image.DOColor(Color.clear, FADE_TIME);
			await tween.ToUniTask();
			tween = null;

			image.enabled = false;
			await UniTask.WaitForEndOfFrame();
		}

		public async UniTask FadeIn() {
			fadeCounter++;

			if (fadeCounter > 1) {
				//return;
			}

			if (tween != null) {
				tween.Kill();
			}

			image.enabled = true;
			image.color = Color.clear;
			tween = image.DOColor(activeColor, FADE_TIME);
			await tween.ToUniTask();
			tween = null;
			await UniTask.WaitForEndOfFrame();
		}
	}
}
