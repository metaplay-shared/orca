using Code.Logbook;
using Game.Logic;
using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Logbook.Tasks {
	public class LogbookChapterSelectorPresenter : MonoBehaviour {
		[SerializeField] private Toggle Toggle;
		[SerializeField] private TMP_Text ChapterNumberText;
		[SerializeField] private Animator Animator;

		[Inject] private LogbookChapterModel chapter;
		[Inject] private ToggleGroup toggleGroup;
		[Inject] private ILogbookTasksController logbookTasksController;

		private static readonly int LockedAnimatorKey = Animator.StringToHash("Locked");
		private static readonly int OpeningAnimatorKey = Animator.StringToHash("Opening");
		private static readonly int OpenAnimatorKey = Animator.StringToHash("Open");

		public event Action ChapterSelected;

		public LogbookChapterModel Chapter => chapter;

		private void Awake() {
			Toggle.group = toggleGroup;
			SetVisuals();
		}

		private void SetVisuals() {
			ChapterNumberText.text = chapter.Info.Index.ToString(CultureInfo.InvariantCulture);
			bool isLocked = chapter.State == ChapterState.Locked;
			Toggle.interactable = !isLocked;

			UpdateAnimator();
		}

		private void UpdateAnimator() {
			Animator.SetBool(LockedAnimatorKey, chapter.State == ChapterState.Locked);
			Animator.SetBool(OpeningAnimatorKey, chapter.State == ChapterState.Opening);
			Animator.SetBool(
				OpenAnimatorKey,
				chapter.State is ChapterState.Open or ChapterState.Complete or ChapterState.RewardClaimed
			);
		}

		private void OnEnable() {
			Toggle.onValueChanged.AddListener(HandleToggleValueChanged);
			logbookTasksController.ChapterUnlocked += HandleChapterUnlocked;
			logbookTasksController.ChapterModified += HandleChapterModified;
		}

		private void OnDisable() {
			Toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
			logbookTasksController.ChapterUnlocked -= HandleChapterUnlocked;
			logbookTasksController.ChapterModified -= HandleChapterModified;
		}

		private void HandleToggleValueChanged(bool isOn) {
			Toggle.interactable = !isOn;
			if (isOn) {
				ChapterSelected?.Invoke();
			}
		}

		private void HandleChapterUnlocked(LogbookChapterId chapterId) {
			if (chapterId == chapter.Info.Id) {
				SetVisuals();
			}
		}

		private void HandleChapterModified(LogbookChapterId chapterId) {
			if (chapterId == chapter.Info.Id) {
				SetVisuals();
			}
		}

		public void SelectChapter() {
			Toggle.isOn = true;
		}
	}
}
