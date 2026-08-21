using Cysharp.Threading.Tasks;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class HighlightElementTriggerAction : TriggerAction {
		[Inject] private Blackout blackout;

		private readonly string type;

		public HighlightElementTriggerAction(string highlightType) {
			type = highlightType;
		}

		public override async UniTask Run() {
			await blackout.HighlightElement(type);
		}
	}
}
