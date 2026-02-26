using Code.UI.Core;
using Code.UI.Events;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity.DefaultIntegration;
using System.Threading;
using UnityEngine;

namespace Code.Logbook {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class OpenDailyTasksTaskOperationProcessor : TaskOperationProcessorBase<OpenDailyTasksOperationInfo> {
		private readonly IUIRootController uiRootController;

		public OpenDailyTasksTaskOperationProcessor(
			IUIRootController uiRootController
		) {
			this.uiRootController = uiRootController;
		}

		public override UniTask Process(
			OpenDailyTasksOperationInfo operation,
			CancellationToken ct
		) {
			DailyTaskEventModel target = MetaplayClient.PlayerModel.GetActiveDailyTaskEventModel();
			if (target == null) {
				Debug.LogWarning($"Couldn't resolve active {nameof(DailyTaskEventModel)}");
			}

			return uiRootController.ShowUI<DailyTasksUIRoot, DailyTasksUIHandle>(
				new DailyTasksUIHandle(target),
				ct
			).OnComplete;
		}
	}
}
