using JetBrains.Annotations;
using UniRx;

namespace Code.Logbook {
	public interface ILogbookController {
		IReadOnlyReactiveProperty<bool> HasActionsToComplete { get; }
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class LogbookController : ILogbookController {
		private readonly IReadOnlyReactiveProperty<bool> hasActionsToComplete;

		public LogbookController(
			IItemDiscoveryController itemDiscoveryController,
			ILogbookTasksController logbookTasksController
		) {
			hasActionsToComplete = Observable.Concat(
				itemDiscoveryController.HasPendingRewards,
				logbookTasksController.HasPendingRewards
			).ToReactiveProperty();
		}

		IReadOnlyReactiveProperty<bool> ILogbookController.HasActionsToComplete => hasActionsToComplete;
	}
}
