using Code.DailyTasks;
using Code.Game;
using Code.HeroTasks;
using Code.Logbook;
using Code.Purchasing;
using Code.UI.Application.Signals;
using Code.UI.CloudCurtains;
using Code.UI.Events;
using Code.UI.Core;
using Code.UI.Core.AndroidBackButton;
using Code.UI.Core.UIBlock;
using Code.UI.Deletion;
using Code.UI.Hud;
using Code.UI.HudBase;
using Code.UI.InfoMessage;
using Code.UI.Island.Signals;
using Code.UI.ItemDiscovery;
using Code.UI.ItemHolder;
using Code.UI.Map;
using Code.UI.Market;
using Code.UI.Merge;
using Code.UI.Merge.AddOns.MergeBoard.LockArea;
using Code.UI.MergeBase;
using Code.UI.Rewarding;
using Code.UI.Shop;
using Code.UI.Tasks;
using Code.UI.Tutorial;
using Code.UI.Tutorial.TutorialPointer;
using Code.UI.Utils;
using Orca.Unity.PlayerLoop;
using UnityEngine;
using Zenject;

namespace Code.UI.Application {
	public class ApplicationInstaller : MonoInstaller {
		[SerializeField] private Canvas canvas;
		[SerializeField] private UIController uiController;
		[SerializeField] private MergeBoardRoot mergeBoard;
		[SerializeField] private Blackout Blackout;
		[SerializeField] private PointerRoot TutorialPointerRoot;
		[SerializeField] private UnityEventMediator UnityEventMediator;


		public static bool IsReady { get; private set; }

		public override void InstallBindings() {
			SignalBusInstaller.Install(Container);

			Container.BindInterfacesTo<GameFlowController>().AsSingle();
			Container.Install<MergeBaseInstaller>();
			Container.Install<MergeInstaller>();
			Container.Install<MapInstaller>();
			Container.Install<TaskInstaller>();
			Container.Install<ItemHolderInstaller>();
			Container.Install<HudInstaller>();
			Container.Install<RewardInstaller>();
			Container.Install<MarketInstaller>();
			Container.Install<ItemDiscoveryInstaller>();
			Container.Install<TutorialInstaller>();
			Container.Install<InfoMessageInstaller>();
			Container.Install<EventsInstaller>();
			Container.Install<PurchasingInstaller>();
			Container.Install<LogbookInstaller>();
			Container.Install<HeroTasksInstaller>();
			Container.Install<DailyTasksInstaller>();
			Container.Install<DeletionInstaller>();

			Container.Install<UIRootInstaller>();
			Container.Install<AndroidBackButtonInstaller>();
			Container.Install<UIBlockInstaller>();
			Container.BindInterfacesTo<UnityEventMediator>().FromInstance(UnityEventMediator).AsSingle();

			Container.Bind<UIController>().FromInstance(uiController).AsSingle();
			Container.BindInterfacesAndSelfTo<CameraControls>()
				.FromInstance(uiController.GetComponent<CameraControls>()).AsSingle();
			Container.BindInterfacesTo<CloudCurtainsPresenter>()
				.FromInstance(FindObjectOfType<CloudCurtainsPresenter>(true))
				.AsSingle();
			Container.Bind<ApplicationInfo>().AsSingle();
			Container.BindInterfacesTo<UnhealthyConnectionIndicator>()
				.FromInstance(FindObjectOfType<UnhealthyConnectionIndicator>(true))
				.AsSingle();

			Container.Bind<PlayerModelClientListener>().AsSingle();
			Container.Bind<ItemCreatedSignalHandler>().AsSingle();
			Container.Install<FrameRateInstaller>();

			Container.BindInstance(canvas).AsSingle();
			Container.BindInstance(mergeBoard).AsSingle();
			Container.BindInstance(Blackout).AsSingle();
			Container.BindInstance(TutorialPointerRoot).AsSingle();

			Container.DeclareSignal<ResourcesChangedSignal>().OptionalSubscriber();
			Container.DeclareSignal<FeatureUnlockedSignal>().OptionalSubscriber();

			// ToDo: Move to own installer
			Container.DeclareSignal<HighlightElementSignal>().OptionalSubscriber();
			Container.DeclareSignal<EnteredIslandSignal>().OptionalSubscriber();
			Container.DeclareSignal<ItemFlightCompletedSignal>().OptionalSubscriber();
			Container.DeclareSignal<LockAreaOpenedSignal>().OptionalSubscriber();
			Container.DeclareSignal<ButtonClickedSignal>().OptionalSubscriber();

			Container.DeclareSignal<ApplicationPauseSignal>().OptionalSubscriber();
			Container.DeclareSignal<ApplicationFocusSignal>().OptionalSubscriber();

			IsReady = true;
		}

		private void OnApplicationQuit() {
			Localizer.PrintReport();
		}
	}
}
