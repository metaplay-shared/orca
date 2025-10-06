using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Code.UI.Application;
using Code.UI.Core.AndroidBackButton;
using Code.UI.Core.UIBlock;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Orca.Common;
using UnityEngine;

namespace Code.UI.Core {
	public interface IUIResult { }

	public interface IUIRootController {
		TUIHandle ShowUI<TUIRoot, TUIHandle>(
			TUIHandle handle,
			CancellationToken ct,
			Option<string> prefabName = default
		)
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle;

		bool IsAnyUIVisible();
	}

	public interface IUIStackItem : IAndroidBackButtonHandler { }

	[UsedImplicitly]
	public class UIRootController : IUIRootController {
		private readonly IUIRootProvider uiRootProvider;
		private readonly IAndroidBackButtonController androidBackButtonController;
		private readonly IUIBlockController uiBlockController;
		private readonly IFrameRateController frameRateController;

		private readonly List<IUIStackItem> uiStack = new List<IUIStackItem>();

		public UIRootController(
			IUIRootProvider uiRootProvider,
			IAndroidBackButtonController androidBackButtonController,
			IUIBlockController uiBlockController,
			IFrameRateController frameRateController
		) {
			this.uiRootProvider = uiRootProvider;
			this.androidBackButtonController = androidBackButtonController;
			this.uiBlockController = uiBlockController;
			this.frameRateController = frameRateController;
		}

		public TUIHandle ShowUI<TUIRoot, TUIHandle>(
			TUIHandle handle,
			CancellationToken ct,
			Option<string> prefabName = default
		)
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle {
			ct.Register(handle.Cancel);
			Present().Forget(); // Cancellation is managed by handle and root
			return handle;

			async UniTask Present() {
				using (uiBlockController.SetState(UIBlockState.Blocked)) {
					foreach (var uiRootProviderHandle in
							await uiRootProvider.LoadUIRoot<TUIRoot, TUIHandle>(handle, prefabName, ct)) {
						using (uiRootProviderHandle) {
							TUIRoot root = uiRootProviderHandle.LoadedUIRoot;
							using (new UIRootScope<TUIRoot, TUIHandle>(root, uiStack))
							using (new AndroidBackButtonScope(androidBackButtonController, root))
							using (var rootCTS = CancellationTokenSource.CreateLinkedTokenSource(
										ct,
										root.GetCancellationTokenOnDestroy()
									)) {
								try {
									Debug.Log(
										$"Begin present of UIRoot with type '{typeof(TUIRoot).Name}'" +
										$"{prefabName.Match(some => $" with prefabName '{some}'", () => string.Empty)}."
									);
									await root.Present(
										handle,
										uiBlockController,
										frameRateController,
										rootCTS.Token);
								} finally {
									rootCTS.Cancel();
								}
							}

							return;
						}
					}
				}

				Debug.LogException(new Exception($"Unable to load UI root with type '{typeof(TUIRoot).Name}'"));
			}
		}

		// HACK: Required by the Controls.cs implementation.
		// Ideally the gameplay will be just another layer on the stack at some point.
		public bool IsAnyUIVisible() => uiStack.Count > 0;

		// The following scope struct enables a safe way to ensure cleanup after exceptions
		// TODO: Should we look into a general solution for scopes in the project?
		private readonly struct UIRootScope<TUIRoot, TUIHandle> : IDisposable
			where TUIHandle : class, IUIHandle
			where TUIRoot : class, IUIRoot<TUIHandle> {
			private readonly IUIRoot<TUIHandle> root;
			private readonly List<IUIStackItem> uiStack;

			public UIRootScope(TUIRoot root, List<IUIStackItem> uiStack) {
				this.root = root;
				this.uiStack = uiStack;
				uiStack.Add(root);
			}

			public void Dispose() {
				uiStack.Remove(root);
			}
		}
	}
}
