using Code.Logbook;
using System.Threading;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Code.UI.Inventory {
	public class OpenLogbookButton : ButtonHelper {
		[SerializeField] private GameObject NotificationIcon;

		[Inject] private ILogbookFlowController logbookFlowController;
		[Inject] private ILogbookController logbookController;

		private void Awake() {
			logbookController.HasActionsToComplete
				.Subscribe(active => NotificationIcon.SetActive(active))
				.AddTo(gameObject);
		}

		protected override void OnClick() {
			logbookFlowController.Run(CancellationToken.None).Forget();
		}
	}
}
