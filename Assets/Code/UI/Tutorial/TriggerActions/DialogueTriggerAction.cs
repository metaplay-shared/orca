using System.Threading;
using Code.UI.Core;
using Code.UI.Dialogue;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class DialogueOpenSignal { }

	public class DialogueTriggerAction : TriggerAction {
		private readonly DialogueId id;

		[Inject] private IUIRootController uiRootController;
		[Inject] private SignalBus signalBus;

		public DialogueTriggerAction(DialogueId id) {
			this.id = id;
		}

		public override async UniTask Run() {
			signalBus.Fire(new DialogueOpenSignal());

			await uiRootController.ShowUI<DialoguePopup, DialoguePopupPayload>(
				new DialoguePopupPayload(id),
				CancellationToken.None
			).OnComplete;
		}
	}
}
