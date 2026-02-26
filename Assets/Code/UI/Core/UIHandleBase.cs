using Cysharp.Threading.Tasks;

namespace Code.UI.Core {
	public interface IUIHandle {
		UniTask OnStart { get; }
		UniTask OnEnterIdle { get; }
		UniTask OnExitIdle { get; }
		UniTask OnComplete { get; }

		void SetStarted();
		void SetEnterIdle();
		void SetExitIdle();
		void SetComplete();

		void Cancel();
	}
	
	public interface IUIHandleWithResult<in TResult> : IUIHandle
		where TResult : IUIResult {
		void SetResult(TResult result);
	}

	public abstract class UIHandleBase : IUIHandle {
		private readonly UniTaskCompletionSource onStartTCS = new UniTaskCompletionSource();
		private readonly UniTaskCompletionSource onEnterIdleTCS = new UniTaskCompletionSource();
		private readonly UniTaskCompletionSource onExitIdleTCS = new UniTaskCompletionSource();
		private readonly UniTaskCompletionSource onCompleteTCS = new UniTaskCompletionSource();

		public UniTask OnStart => onStartTCS.Task;
		public UniTask OnEnterIdle => onEnterIdleTCS.Task;
		public UniTask OnExitIdle => onExitIdleTCS.Task;
		public UniTask OnComplete => onCompleteTCS.Task;

		void IUIHandle.SetStarted() {
			onStartTCS.TrySetResult();
		}

		void IUIHandle.SetEnterIdle() {
			onEnterIdleTCS.TrySetResult();
		}

		void IUIHandle.SetExitIdle() {
			onExitIdleTCS.TrySetResult();
		}

		void IUIHandle.SetComplete() {
			onCompleteTCS.TrySetResult();
		}
		
		void IUIHandle.Cancel() {
			onStartTCS.TrySetCanceled();
			onEnterIdleTCS.TrySetCanceled();
			onExitIdleTCS.TrySetCanceled();
			onCompleteTCS.TrySetCanceled();
			
			OnCancel();
		}

		protected virtual void OnCancel() { }
	}

	public abstract class UIHandleWithResultBase<TResult> : UIHandleBase, IUIHandleWithResult<TResult>
		where TResult : IUIResult {
		private readonly UniTaskCompletionSource<TResult> onResultTCS = new UniTaskCompletionSource<TResult>();

		public UniTask<TResult> OnResult => onResultTCS.Task;
		public UniTask<TResult> OnCompleteWithResult => CompleteWithResult();

		private async UniTask<TResult> CompleteWithResult() {
			var result = await OnResult;
			await OnComplete;
			return result;
		}

		void IUIHandleWithResult<TResult>.SetResult(TResult result) {
			onResultTCS.TrySetResult(result);
		}

		protected override void OnCancel() {
			base.OnCancel();

			onResultTCS.TrySetCanceled();
		}
	}
}
