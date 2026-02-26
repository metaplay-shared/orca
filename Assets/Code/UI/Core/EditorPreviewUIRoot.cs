#if UNITY_EDITOR
using System;
using System.Threading;
using Code.UI.Application;
using Code.UI.Core.AndroidBackButton;
using Code.UI.Core.UIBlock;
using Cysharp.Threading.Tasks;
using Orca.Common;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Code.UI.Core {
	/// <summary>
	/// This UI root behaviour is not meant to be used in content released with the game.
	/// It is a self running UI root behaviour used by artists to preview UI.
	/// </summary>
	public class EditorPreviewUIRoot : UIRootBase<EditorPreviewUIRoot.EditorPreviewUIRootHandle>,
		IUIRootProvider,
		IAndroidBackButtonController,
		IUIBlockController,
		IFrameRateController {
		public class EditorPreviewUIRootHandle : UIHandleBase { }

		private void Awake() {
			async UniTask ShowUI() {
				var controller = new UIRootController(
					uiRootProvider: this,
					androidBackButtonController: this,
					uiBlockController: this,
					frameRateController: this
				);
				await controller.ShowUI<EditorPreviewUIRoot, EditorPreviewUIRootHandle>(
					new EditorPreviewUIRootHandle(),
					CancellationToken.None
				).OnComplete;
			}
			
			ShowUI().Forget();
		}

		protected override void Init() { }

		protected override UniTask Idle(CancellationToken ct) => UniTask.Never(ct);

		protected override void HandleAndroidBackButtonPressed() { }

		void IAndroidBackButtonController.AddBackButtonHandler(IAndroidBackButtonHandler handler) { }
		void IAndroidBackButtonController.RemoveBackButtonHandler(IAndroidBackButtonHandler handler) { }
		AndroidBackButtonLock IAndroidBackButtonController.LockBackButton() => new AndroidBackButtonLock(this);
		void IAndroidBackButtonController.UnlockBackButton(AndroidBackButtonLock backButtonLock) { }

		UIBlock.UIBlock IUIBlockController.SetState(
			UIBlockState state,
			string memberName,
			string sourceFilePath,
			int sourceLineNumber
		) {
			return new UIBlock.UIBlock(state, string.Empty, this);
		}

		void IUIBlockController.RemoveBlock(UIBlock.UIBlock block) { }
		UIBlockState IUIBlockController.CurrentState => UIBlockState.Unblocked;

		IDisposable IFrameRateController.RequestTargetFrameRate(
			int frameRate,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0
		) {
			return new MockDisposable();
		}

		public IEnumerable<IFrameRateHandle> GetHandles() {
			return Enumerable.Empty<IFrameRateHandle>();
		}

		private class MockDisposable : IDisposable {
			public void Dispose() { }
		}

		public UniTask PreloadUIRoot<TUIRoot, TUIHandle>(CancellationToken ct) 
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle> 
			where TUIHandle : class, IUIHandle {
			return UniTask.CompletedTask;
		}

		public UniTask<Option<IUIRootProviderHandle<TUIRoot, TUIHandle>>> LoadUIRoot<TUIRoot, TUIHandle>(
			TUIHandle uiHandle,
			Option<string> prefabName,
			CancellationToken ct
		) 
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle> 
			where TUIHandle : class, IUIHandle {
			return UniTask.FromResult(
				new Option<IUIRootProviderHandle<TUIRoot, TUIHandle>>(new Handle<TUIRoot, TUIHandle>(this as TUIRoot))
			);
		}

		public bool UIRootExists(string prefabName) {
			return true;
		}

		class Handle<TUIRoot, TUIHandle> : IUIRootProviderHandle<TUIRoot, TUIHandle>
			where TUIRoot : class, IUIRoot<TUIHandle> 
			where TUIHandle : class, IUIHandle {

			public Handle(TUIRoot loadedUIRoot) {
				LoadedUIRoot = loadedUIRoot;
			}
			
			public void Dispose() {
				
			}

			public TUIRoot LoadedUIRoot { get; }
		}
	}
}
#endif
