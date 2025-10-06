using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orca.Common;
using System.IO;
using System.Runtime.CompilerServices;
using Zenject;

namespace Code.UI.Application {
	public interface IFrameRateController {
		IDisposable RequestTargetFrameRate(
			int frameRate,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0
		);

		IEnumerable<IFrameRateHandle> GetHandles();
	}

	public interface IFrameRateHandle {
		string Context { get; }
	}

	[UsedImplicitly]
	public class FrameRateController : IFrameRateController, IInitializable {
		private const int DEFAULT_FRAME_RATE = 30;

		private readonly List<Handle> handles = new();

		public IDisposable RequestTargetFrameRate(
			int frameRate,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0
		) {
			// ReSharper disable once RedundantAssignment
			var context = string.Empty;
			#if UNITY_EDITOR || DEVELOPMENT_BUILD
			context = $"{Path.GetFileName(sourceFilePath)}:{sourceLineNumber} - {memberName}";
			#endif
			Handle handle = new Handle(this, frameRate, context);
			handles.Add(handle);
			UpdateFPS();
			return handle;
		}

		public IEnumerable<IFrameRateHandle> GetHandles() {
			return handles;
		}

		private void UpdateFPS() {
			UnityEngine.Application.targetFrameRate = handles
				.LastOrDefault()
				.ToOption()
				.Map(h => h.FrameRate)
				.GetOrElse(DEFAULT_FRAME_RATE);
		}

		private void RemoveHandle(Handle handle) {
			handles.Remove(handle);
			UpdateFPS();
		}

		private class Handle : IDisposable, IFrameRateHandle {
			private readonly FrameRateController controller;
			public readonly int FrameRate;
			public string Context { get; }

			public Handle(
				FrameRateController frameRateController,
				int frameRate,
				string context
			) {
				controller = frameRateController;
				FrameRate = frameRate;
				Context = context;
			}

			public void Dispose() {
				controller.RemoveHandle(this);
			}
		}

		public void Initialize() {
			UnityEngine.Application.targetFrameRate = DEFAULT_FRAME_RATE;
		}
	}

	public static class FrameRateControllerExtensions {
		public static IDisposable RequestHighFPS(
			this IFrameRateController frameRateController,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0
		) {
			// ReSharper disable ExplicitCallerInfoArgument
			return frameRateController.RequestTargetFrameRate(60, memberName, sourceFilePath, sourceLineNumber);
			// ReSharper restore ExplicitCallerInfoArgument
		}
	}
}
