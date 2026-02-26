using System;
using System.Threading;
using Code.UI.Core;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Metaplay.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Deletion {
	public class DeletionScheduledPayload : UIHandleBase {
	}

	public class DeletionScheduledUIRoot : UIRootBase<DeletionScheduledPayload> {
		[SerializeField] private TMP_Text TimeLeftLabel;
		[SerializeField] private Button CancelButton;
		[Inject] private IDeletionController deletionController;

		protected override void Init() {
		}

		protected override UniTask Idle(CancellationToken ct) {
			return deletionController.WaitForCancellation(ct);
		}

		private void Update() {
			MetaDuration timeLeft = deletionController.TimeLeft;
			if (timeLeft > MetaDuration.Zero) {
				TimeLeftLabel.text = timeLeft.ToSimplifiedString();
			} else {
				TimeLeftLabel.text = Localizer.Localize("Info.DeletionPending");
				CancelButton.interactable = false;
			}
		}

		protected override void HandleAndroidBackButtonPressed() {
		}

		public void DeletionCancelled() {
			deletionController.CancelDeletion();
		}
	}
}
