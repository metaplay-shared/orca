using Code.UI.Events;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity.DefaultIntegration;
using System;
using UniRx;
using Zenject;

namespace Code.DailyTasks {
	public interface IDailyTasksController {
		IReadOnlyReactiveProperty<bool> HasSomethingToClaim { get; }
		[CanBeNull] DailyTaskEventModel GetDailyTaskEventModel();
		void NotifyDailyTaskProgressionMade(EventId eventId, int progressAmount, ResourceModificationContext context);
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class DailyTasksController : IDailyTasksController, IInitializable, IDisposable {
		private readonly CompositeDisposable disposables = new();
		private readonly SignalBus signalBus;

		public DailyTasksController(
			SignalBus signalBus
		) {
			this.signalBus = signalBus;
			HasSomethingToClaim = new ReactiveProperty<bool>(GetHasSomethingToClaim());
		}

		private ReactiveProperty<bool> HasSomethingToClaim { get; }

		public DailyTaskEventModel GetDailyTaskEventModel() {
			return MetaplayClient.PlayerModel.GetActiveDailyTaskEventModel();
		}

		public void NotifyDailyTaskProgressionMade(
			EventId eventId,
			int progressAmount,
			ResourceModificationContext context
		) {
			HandleEventStateChanged();
		}

		IReadOnlyReactiveProperty<bool> IDailyTasksController.HasSomethingToClaim => HasSomethingToClaim;

		public void Dispose() {
			disposables.Dispose();
		}

		public void Initialize() {
			Observable.FromEvent(
					handler => signalBus.Subscribe<EventStateChangedSignal>(handler),
					handler => signalBus.Unsubscribe<EventStateChangedSignal>(handler)
				).Subscribe(_ => HandleEventStateChanged())
				.AddTo(disposables);
		}

		private void HandleEventStateChanged() {
			HasSomethingToClaim.Value = GetHasSomethingToClaim();
		}

		private bool GetHasSomethingToClaim() {
			DailyTaskEventModel model = GetDailyTaskEventModel();
			return model?.UnclaimedRewards() > 0;
		}
	}
}
