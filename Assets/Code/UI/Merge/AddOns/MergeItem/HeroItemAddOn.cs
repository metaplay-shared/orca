using System.Collections.Generic;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class HeroItemAddOn : ItemAddOn {
		[SerializeField] private GameObject SpeechBubble;
		[SerializeField] private Image Icon;
		[SerializeField] private Sprite ReadyToClaimIcon;
		[SerializeField] private Sprite ProcessingTimerIcon;
		[SerializeField] private Sprite WaitingForResourcesIcon;
		[SerializeField] private Animator Animator;

		private static readonly int StateChanged = Animator.StringToHash("StateChanged");

		private float speechBubbleTimer;
		private List<HeroTypeId> heroes = new List<HeroTypeId>();

		private bool CanCompleteTask {
			get {
				foreach (HeroTypeId hero in heroes) {
					HeroTaskModel task = MetaplayClient.PlayerModel.Heroes.Heroes[hero].CurrentTask;
					if (task != null &&
						task.State == HeroTaskState.Created &&
						MetaplayClient.PlayerModel.Inventory.HasEnoughResources(task.Info)) {
						return true;
					}
				}

				return false;
			}
		}

		private bool HasState(HeroTaskState state) {
			foreach (HeroTypeId hero in heroes) {
				HeroTaskModel task = MetaplayClient.PlayerModel.Heroes.Heroes[hero].CurrentTask;
				if (task != null && task.State == state) {
					return true;
				}
			}

			return false;
		}

		protected override void Setup() {
			OnStateChanged();
		}

		public override void OnStateChanged() {
			heroes.Clear();
			foreach (HeroModel hero in MetaplayClient.PlayerModel.Heroes.HeroesInBuilding(ItemModel.Info.Type)) {
				heroes.Add(hero.Info.Type);
			}
		}

		private void Awake() {
			gameObject.UpdateAsObservable()
				.Select(_ => ResolveState())
				.ToReactiveProperty()
				.Subscribe(HandleStateChanged)
				.AddTo(gameObject);
		}

		private State ResolveState() {
			return CanCompleteTask
				? State.CanComplete
				: HasState(HeroTaskState.Fulfilled)
					? State.Processing
					: HasState(HeroTaskState.Finished)
						? State.ReadyToClaim
						: State.Inactive;
		}

		private void HandleStateChanged(State state) {
			bool active = state is not State.Inactive;
			SpeechBubble.SetActive(active);

			if (!active) {
				return;
			}

			Animator.SetTrigger(StateChanged);
			Icon.sprite = state switch {
				State.CanComplete  => WaitingForResourcesIcon,
				State.Processing   => ProcessingTimerIcon,
				State.ReadyToClaim => ReadyToClaimIcon,
				_                  => null
			};
		}

		private enum State {
			Inactive,
			CanComplete,
			Processing,
			ReadyToClaim
		}
	}
}
