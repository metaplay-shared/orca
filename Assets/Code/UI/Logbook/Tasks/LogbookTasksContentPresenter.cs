using Code.Logbook;
using Game.Logic;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Logbook.Tasks {
	public class LogbookTasksContentPresenter : MonoBehaviour {
		[Header("Chapter Selecting")]
		[SerializeField] private LogbookChapterSelectorPresenter TemplateChapterSelector;
		[SerializeField] private Transform ChapterSelectorContainer;
		[SerializeField] private ToggleGroup SelectorsToggleGroup;

		[Header("Chapter Content")]
		[SerializeField] private LogbookChapterContentPresenter ChapterContentPresenter;

		[Inject] private DiContainer container;
		[Inject] private ILogbookTasksController logbookTasksController;

		private readonly List<LogbookChapterSelectorPresenter> chapterSelectorPresenters = new();

		public void OnEnable() {
			PopulateChapterSelectors();
			SelectInitialChapter();
		}

		private void SelectInitialChapter() {
			SelectorsToggleGroup.SetAllTogglesOff(false);
			LogbookChapterModel initialChapter = GetInitialChapterSelection();
			foreach (
				LogbookChapterSelectorPresenter presenter
				in chapterSelectorPresenters.Where(presenter => presenter.Chapter == initialChapter)
			) {
				presenter.SelectChapter();
				break;
			}
		}

		private LogbookChapterModel GetInitialChapterSelection() {
			return logbookTasksController.GetChapters()
				.Where(it => it.State != ChapterState.Locked)
				.Aggregate((it, sel) => it.Info.Index > sel.Info.Index ? it : sel);
		}

		private void PopulateChapterSelectors() {
			DiContainer selectorsContainer = container.CreateSubContainer();
			selectorsContainer.Bind<ToggleGroup>().FromInstance(SelectorsToggleGroup).AsSingle();

			foreach (LogbookChapterSelectorPresenter chapterSelector in chapterSelectorPresenters) {
				Destroy(chapterSelector.gameObject);
			}
			chapterSelectorPresenters.Clear();

			foreach (LogbookChapterModel chapter in logbookTasksController.GetChapters()) {
				DiContainer chapterContainer = selectorsContainer.CreateSubContainer();
				chapterContainer.BindInstance(chapter).AsSingle();
				LogbookChapterSelectorPresenter chapterSelector =
					chapterContainer.InstantiatePrefabForComponent<LogbookChapterSelectorPresenter>(
						TemplateChapterSelector,
						ChapterSelectorContainer
					);
				chapterSelector.ChapterSelected += () => ChapterContentPresenter.SelectChapter(chapter);
				chapterSelectorPresenters.Add(chapterSelector);
			}
		}
	}
}
