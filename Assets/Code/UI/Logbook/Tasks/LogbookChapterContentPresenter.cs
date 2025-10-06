using Code.Logbook;
using Code.UI.Dialogue;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Logic;
using Orca.Unity.Utilities;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI.Logbook.Tasks {
	public class LogbookChapterContentPresenter : MonoBehaviour {
		[SerializeField] private TMP_Text Title;
		[SerializeField] private TMP_Text Story;
		[SerializeField] private LogbookTaskProgressionPresenter LogbookTaskProgressionPresenter;
		[SerializeField] private LogbookTasksPresenter LogbookTasksPresenter;

		[Inject] private ILogbookTasksController logbookTasksController;

		public void SelectChapter(LogbookChapterModel chapter) {
			bool isLocked = chapter.State == ChapterState.Locked;
			Title.text = Localizer.Localize($"Logbook.ChapterTitle.{chapter.Info.Id.Value}");
			Story.text = !isLocked
				? Localizer.Localize($"Logbook.ChapterStory.{chapter.Info.Id.Value}")
				: Localizer.Localize("Logbook.ChapterStory.Locked");
			LogbookTaskProgressionPresenter.Setup(chapter);
			LogbookTasksPresenter.Setup(chapter);

			AnimateOpening(chapter.State == ChapterState.Opening);

			logbookTasksController.NotifyChapterOpened(chapter.Info.Id);
		}

		private void AnimateOpening(bool firstTimeOpening) {
			if (!firstTimeOpening) {
				CanvasGroup canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
				canvasGroup.DOFade(1f, 0.25f).ChangeStartValue(0f);
				return;
			}

			const float APPEAR_INTERVAL = 0.325f;
			const float FADE_DURATION = 0.5f;
			Sequence sequence = DOTween.Sequence();
			sequence.Insert(
				0 * APPEAR_INTERVAL,
				Title.GetOrAddComponent<CanvasGroup>()
					.DOFade(1f, FADE_DURATION)
					.ChangeStartValue(0f)
			);
			sequence.Insert(
				1 * APPEAR_INTERVAL,
				Story.GetOrAddComponent<CanvasGroup>()
					.DOFade(1f, FADE_DURATION)
					.ChangeStartValue(0f)
			);
			sequence.InsertCallback(
				1 * APPEAR_INTERVAL,
				() => Story.GetOrAddComponent<TextFader>()
					.FadeIn(Story.gameObject.GetCancellationTokenOnDestroy())
					.Forget()
			);
			sequence.Insert(
				2 * APPEAR_INTERVAL,
				LogbookTaskProgressionPresenter.GetOrAddComponent<CanvasGroup>()
					.DOFade(1f, FADE_DURATION)
					.ChangeStartValue(0f)
			);
			sequence.Insert(
				3 * APPEAR_INTERVAL,
				LogbookTasksPresenter.GetOrAddComponent<CanvasGroup>()
					.DOFade(1f, FADE_DURATION)
					.ChangeStartValue(0f)
			);
		}
	}
}
