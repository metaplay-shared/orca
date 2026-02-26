using System.Threading;
using Cysharp.Threading.Tasks;

namespace Code.UI.Tutorial.TriggerActions {
	public abstract class TriggerAction {
		public abstract UniTask Run();
	}
}
