using Code.UI;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using System.Threading;

namespace Code.Logbook {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class OpenIslandTaskOperationProcessor : TaskOperationProcessorBase<OpenIslandOperationInfo> {
		private readonly UIController uiController;

		public OpenIslandTaskOperationProcessor(
			UIController uiController
		) {
			this.uiController = uiController;
		}

		public override UniTask Process(OpenIslandOperationInfo operation, CancellationToken ct) {
			return uiController.GoToIslandAsync(operation.Island, ct);
		}
	}
}
