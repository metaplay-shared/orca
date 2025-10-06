using System;

namespace Code.UI.Application {
	public static class LoadingInfo {
		public static Func<float> ProgressGetter { get; set; }
		public static float Progress => ProgressGetter?.Invoke() ?? 0f;
	}
}
