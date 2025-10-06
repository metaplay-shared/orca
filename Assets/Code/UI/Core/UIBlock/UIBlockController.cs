using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Code.UI.Core.AndroidBackButton;
using Code.UI.Core.UIBlock.UI;
using Orca.Common;
using UnityEngine;

namespace Code.UI.Core.UIBlock {
	public interface IUIBlockController {
		/// <summary>
		/// Adds a new UI block state to the stack. The last state added is applied.
		/// The last 3 arguments are filled by the compiler for easier debugging when many
		/// states are added.
		/// </summary>
		/// <param name="state">The UI block state that should be applied right away</param>
		/// <param name="memberName">Provided by the compiler, do not use this.</param>
		/// <param name="sourceFilePath">Provided by the compiler, do not use this.</param>
		/// <param name="sourceLineNumber">Provided by the compiler, do not use this.</param>
		/// <returns>A disposable state object that will remove
		/// the added state from the stack once it is disposed</returns>
		UIBlock SetState(
			UIBlockState state,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0
		);

		void RemoveBlock(UIBlock block);
		
		UIBlockState CurrentState { get; }
	}

	public class UIBlockController : IUIBlockController, IDisposable {
		private readonly IUIBlockOverlayProvider uiBlockOverlayProvider;
		private readonly IAndroidBackButtonController androidBackButtonController;
		private Option<IUIBlockOverlay> uiBlockOverlay;
		private readonly List<UIBlock> uiBlocks = new List<UIBlock>();
		private Option<AndroidBackButtonLock> androidBackButtonLock;

		public UIBlockController(
			IUIBlockOverlayProvider uiBlockOverlayProvider,
			IAndroidBackButtonController androidBackButtonController) {
			this.uiBlockOverlayProvider = uiBlockOverlayProvider;
			this.androidBackButtonController = androidBackButtonController;
		}

		public UIBlock SetState(
			UIBlockState state,
			[CallerMemberName] string memberName = "",
			[CallerFilePath] string sourceFilePath = "",
			[CallerLineNumber] int sourceLineNumber = 0
		) {
			var name = $"{Path.GetFileName(sourceFilePath)} ({sourceLineNumber.ToString()}): {memberName}";
			Debug.Log($"Setting UIBlock state: {state} by {name}");

			if (!uiBlockOverlay.HasValue) {
				uiBlockOverlay = uiBlockOverlayProvider.LoadUIOverlay();
				uiBlockOverlay.MatchSome(o => o.DontDestroyOnLoad());
			}

			var uiBlock = new UIBlock(state, name, this);

			foreach (var overlay in uiBlockOverlay) {
				overlay.SetState(state);
			}
			
			UpdateAndroidBackButtonLock(state);

			uiBlocks.Add(uiBlock);

			return uiBlock;
		}

		public void RemoveBlock(UIBlock block) {
			if (!uiBlocks.Remove(block)) {
				Debug.LogWarning($"{nameof(UIBlockController)}: Tried to remove unknown UIBlock: {block.Name}.");
			}

			Debug.Log($"Removing UIBlock: {block.Name}");

			var newState = CurrentState;

			foreach (var overlay in uiBlockOverlay) {
				overlay.SetState(newState);
			}
			
			UpdateAndroidBackButtonLock(newState);
		}

		public UIBlockState CurrentState =>
			uiBlocks.LastOrDefault()
				.ToOption()
				.Map(b => b.State)
				.GetOrElse(UIBlockState.Unblocked);

		private void UpdateAndroidBackButtonLock(UIBlockState state) {
			if (state == UIBlockState.Unblocked) {
				androidBackButtonLock.MatchSome(l => l.Dispose());
				androidBackButtonLock = default;
			} else {
				if (!androidBackButtonLock.HasValue) {
					androidBackButtonLock = androidBackButtonController.LockBackButton();
				}
			}
		}

		public void Dispose() {
			foreach (var overlay in uiBlockOverlay) {
				overlay.Destroy();
				uiBlockOverlay = default;
			}
		}
	}

	public enum UIBlockState {
		Unblocked,
		Blocked,
		Overlay
	}

	public class UIBlock : IDisposable {
		public readonly UIBlockState State;
		public readonly string Name;

		private readonly IUIBlockController uiBlockController;

		public UIBlock(UIBlockState state, string name, IUIBlockController uiBlockController) {
			State = state;
			Name = name;
			this.uiBlockController = uiBlockController;
		}

		public void Dispose() {
			uiBlockController.RemoveBlock(this);
		}
	}
}
