namespace Code.UI.Core.Editor {
	public static class UIHandleUnitTestExtensions {
		public static TUIHandle SetComplete<TUIHandle>(this TUIHandle handle)
			where TUIHandle : IUIHandle {
			CompleteUIHandle(handle);
			return handle;
		}
		
		public static TUIHandle SetComplete<TUIHandle, TUIResult>(this TUIHandle handle, TUIResult result)
			where TUIHandle : IUIHandleWithResult<TUIResult> 
			where TUIResult : IUIResult {
			CompleteUIHandleWithResult(handle, result);
			return handle;
		}

		private static void CompleteUIHandle(IUIHandle handle)
		{
			handle.SetStarted();
			handle.SetEnterIdle();
			handle.SetExitIdle();
			handle.SetComplete();
		}

		private static void CompleteUIHandleWithResult<TUIResult>(
			IUIHandleWithResult<TUIResult> handle,
			TUIResult result)
			where TUIResult : IUIResult
		{
			handle.SetResult(result);
			CompleteUIHandle(handle);
		}
	}
}
