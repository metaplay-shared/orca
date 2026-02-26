using Code.UI.Core.AndroidBackButton;
using Code.UI.Core.UIBlock.UI;
//using NSubstitute;
using NUnit.Framework;
using Orca.Common;

namespace Code.UI.Core.UIBlock.Editor {
	// public class UIBlockControllerTests {
	// 	private UIBlockController uiBlockController;
	// 	private IUIBlockOverlayProvider uiBlockOverlayProvider;
	// 	private IAndroidBackButtonController androidBackButtonController;
	//
	// 	[SetUp]
	// 	public void SetUp() {
	// 		uiBlockOverlayProvider = Substitute.For<IUIBlockOverlayProvider>();
	// 		androidBackButtonController = Substitute.For<IAndroidBackButtonController>();
	// 		uiBlockController = new UIBlockController(uiBlockOverlayProvider, androidBackButtonController);
	// 	}
	//
	// 	[Test]
	// 	public void TestSetStateLoadsProvider() {
	// 		uiBlockController.SetState(UIBlockState.Blocked);
	// 		uiBlockOverlayProvider.Received().LoadUIOverlay();
	// 	}
	//
	// 	[Test]
	// 	public void TestSettingMultipleStates() {
	// 		var uiBlockOverlay = Substitute.For<IUIBlockOverlay>();
	// 		uiBlockOverlayProvider.LoadUIOverlay().Returns(uiBlockOverlay.ToOption());
	// 		using (uiBlockController.SetState(UIBlockState.Blocked)) {
	// 			uiBlockOverlay.Received().SetState(UIBlockState.Blocked);
	// 			using (uiBlockController.SetState(UIBlockState.Unblocked)) {
	// 				uiBlockOverlay.Received().SetState(UIBlockState.Unblocked);
	// 				using (uiBlockController.SetState(UIBlockState.Overlay)) {
	// 					uiBlockOverlay.Received().SetState(UIBlockState.Overlay);
	// 					uiBlockOverlay.ClearReceivedCalls();
	// 				}
	//
	// 				uiBlockOverlay.Received().SetState(UIBlockState.Unblocked);
	// 				uiBlockOverlay.ClearReceivedCalls();
	// 			}
	//
	// 			uiBlockOverlay.Received().SetState(UIBlockState.Blocked);
	// 			uiBlockOverlay.ClearReceivedCalls();
	// 		}
	//
	// 		uiBlockOverlay.Received().SetState(UIBlockState.Unblocked);
	// 		uiBlockOverlay.ClearReceivedCalls();
	// 	}
	//
	// 	[Test]
	// 	public void TestRemovingStatesInNonRegularOrder() {
	// 		var uiBlockOverlay = Substitute.For<IUIBlockOverlay>();
	// 		uiBlockOverlayProvider.LoadUIOverlay().Returns(uiBlockOverlay.ToOption());
	//
	// 		var firstBlock = uiBlockController.SetState(UIBlockState.Blocked);
	// 		uiBlockOverlay.Received().SetState(UIBlockState.Blocked);
	// 		uiBlockOverlay.ClearReceivedCalls();
	//
	// 		var secondBlock = uiBlockController.SetState(UIBlockState.Overlay);
	// 		uiBlockOverlay.Received().SetState(UIBlockState.Overlay);
	// 		uiBlockOverlay.ClearReceivedCalls();
	//
	// 		var thirdBlock = uiBlockController.SetState(UIBlockState.Unblocked);
	// 		uiBlockOverlay.Received().SetState(UIBlockState.Unblocked);
	// 		uiBlockOverlay.ClearReceivedCalls();
	//
	// 		firstBlock.Dispose();
	// 		uiBlockOverlay.Received().SetState(UIBlockState.Unblocked);
	// 		uiBlockOverlay.ClearReceivedCalls();
	//
	// 		thirdBlock.Dispose();
	// 		uiBlockOverlay.Received().SetState(UIBlockState.Overlay);
	// 		uiBlockOverlay.ClearReceivedCalls();
	//
	// 		secondBlock.Dispose();
	// 		uiBlockOverlay.Received().SetState(UIBlockState.Unblocked);
	// 		uiBlockOverlay.ClearReceivedCalls();
	// 	}
	//
	// 	[Test]
	// 	public void TestAndroidBackButtonLocking() {
	// 		androidBackButtonController.LockBackButton()
	// 			.Returns(new AndroidBackButtonLock(androidBackButtonController));
	// 		using (uiBlockController.SetState(UIBlockState.Blocked)) {
	// 			androidBackButtonController.Received().LockBackButton();
	// 			using (uiBlockController.SetState(UIBlockState.Unblocked)) {
	// 				androidBackButtonController.ReceivedWithAnyArgs().UnlockBackButton(default);
	// 				androidBackButtonController.ClearReceivedCalls();
	// 			}
	//
	// 			androidBackButtonController.Received().LockBackButton();
	// 			androidBackButtonController.ClearReceivedCalls();
	// 		}
	//
	// 		androidBackButtonController.ReceivedWithAnyArgs().UnlockBackButton(default);
	// 		androidBackButtonController.ClearReceivedCalls();
	//
	// 		using (uiBlockController.SetState(UIBlockState.Unblocked)) {
	// 			androidBackButtonController.DidNotReceive().LockBackButton();
	// 		}
	//
	// 		androidBackButtonController.DidNotReceiveWithAnyArgs().UnlockBackButton(default);
	// 	}
	// }
}
