using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using System;
using System.Threading;

namespace Code.Logbook {
	public interface ILogbookTaskOperationProcessor {
		UniTask Process(object operation, CancellationToken ct);
	}

	public interface ILogbookTaskOperationProcessor<in TOperation> {
		[UsedImplicitly]
		UniTask Process(TOperation operation, CancellationToken ct);
	}

	public abstract class TaskOperationProcessorBase : ILogbookTaskOperationProcessor {
		public abstract UniTask Process(object operation, CancellationToken ct);
	}

	public abstract class TaskOperationProcessorBase<TOperation> :
		TaskOperationProcessorBase,
		ILogbookTaskOperationProcessor<TOperation>
		where TOperation : LogbookTaskOperationInfo {
		public abstract UniTask Process(TOperation operation, CancellationToken ct);

		public sealed override UniTask Process(object operation, CancellationToken ct) {
			if (operation is not TOperation typedOperation) {
				throw new ArgumentException($"Given argument is not type of '{typeof(TOperation).Name}'");
			}

			return Process(typedOperation, ct);
		}
	}
}
