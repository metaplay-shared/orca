using Code.UI.Core;
using Code.UI.Logbook;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace Code.Logbook {
	public interface ILogbookFlowController {
		UniTask Run(CancellationToken ct);
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class LogbookFlowController : ILogbookFlowController {
		private readonly IUIRootController uiRootController;
		private readonly DiContainer container;

		public LogbookFlowController(
			IUIRootController uiRootController,
			DiContainer container
		) {
			this.uiRootController = uiRootController;
			this.container = container;
		}

		public async UniTask Run(CancellationToken ct) {
			LogbookUIHandle handle = uiRootController.ShowUI<LogbookUIRoot, LogbookUIHandle>(
				new LogbookUIHandle(),
				ct
			);
			LogbookUIResult result = await handle.OnCompleteWithResult;
			switch (result) {
				case NavigateToTaskResult navigationResult:
					await ProcessTaskOperations(navigationResult.TaskModel, ct);
					break;
			}
		}

		private async UniTask ProcessTaskOperations(LogbookTaskModel taskModel, CancellationToken ct) {
			foreach (LogbookTaskOperationInfo operation in taskModel.Info.Operations) {
				Type operationType = operation.GetType();
				Type processorType = typeof(ILogbookTaskOperationProcessor<>).MakeGenericType(operationType);
				ILogbookTaskOperationProcessor processor =
					(ILogbookTaskOperationProcessor)container.TryResolve(processorType);

				if (processor == null) {
					Debug.LogWarning($"Couldn't process task operation of type: '{operationType}'");
					Debug.LogWarning("Interrupting task operation processing");
					break;
				}

				await processor.Process(operation, ct);
			}
		}
	}
}
