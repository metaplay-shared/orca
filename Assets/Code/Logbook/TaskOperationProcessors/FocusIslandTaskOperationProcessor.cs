using Code.UI;
using Code.UI.Application;
using Code.UI.Map;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using System.Threading;

namespace Code.Logbook {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class FocusIslandTaskOperationProcessor : TaskOperationProcessorBase<FocusIslandOperationInfo> {
		private readonly ApplicationInfo applicationInfo;
		private readonly UIController uiController;
		private readonly CameraControls cameraControls;

		public FocusIslandTaskOperationProcessor(
			ApplicationInfo applicationInfo,
			UIController uiController,
			CameraControls cameraControls
		) {
			this.applicationInfo = applicationInfo;
			this.uiController = uiController;
			this.cameraControls = cameraControls;
		}

		public override async UniTask Process(
			FocusIslandOperationInfo operation,
			CancellationToken ct
		) {
			if (applicationInfo.ActiveIsland.Value != null) {
				await uiController.LeaveIsland(ct);
			}

			await cameraControls.FocusIslandAsync(operation.Island, false, ct);
		}
	}
}
