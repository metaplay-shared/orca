using System;
using Code.UI.Tasks.Hero;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.Hero {
	public class HeroButton : MonoBehaviour {
		[SerializeField] private GameObject Checkmark;
		[SerializeField] private GameObject Lock;

		[SerializeField] private Image Portrait;

		[Inject] private SignalBus signalBus;

		private HeroModel hero;

		public void SetLocked() {
			Checkmark.SetActive(false);
			Lock.SetActive(true);

			Portrait.gameObject.SetActive(false);
		}

		public void SetHero(HeroModel hero) {
			this.hero = hero;
			Lock.SetActive(false);

			Portrait.gameObject.SetActive(true);

			Checkmark.SetActive(CanComplete());
		}

		private void OnEnable() {
			signalBus.Subscribe<HeroTaskModifiedSignal>(OnHeroTaskModified);
		}

		private void OnDisable() {
			signalBus.Unsubscribe<HeroTaskModifiedSignal>(OnHeroTaskModified);
		}

		private void OnHeroTaskModified(HeroTaskModifiedSignal signal) {
			if (hero == null) {
				return;
			}

			if (hero.Info.Type != signal.HeroType) {
				return;
			}

			Checkmark.SetActive(CanComplete());
		}
		
		private bool CanComplete() {
			if (hero == null) {
				return false;
			}

			return MetaplayClient.PlayerModel.Inventory.HasEnoughResources(hero.CurrentTask.Info) &&
				hero.CurrentTask.State == HeroTaskState.Created;
		}
	}
}
