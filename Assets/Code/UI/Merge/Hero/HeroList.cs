using System;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Merge.Hero {
	public class HeroList : MonoBehaviour {
		public Action<HeroModel> HeroSelected;

		[SerializeField] private GameObject TemplateHeroButton;

		[Inject] private DiContainer container;

		public void Setup() {
			CreateButtons();
		}

		private void CreateButtons() {
			foreach (var taskGiverInfo in MetaplayClient.PlayerModel.GameConfig.Heroes.Values) {
				var instance = container.InstantiatePrefab(TemplateHeroButton, transform);
				var heroButton = instance.GetComponent<HeroButton>();
				
				// Return after one locked hero is shown
				if (!MetaplayClient.PlayerModel.Heroes.Heroes.ContainsKey(taskGiverInfo.Type)) {
					heroButton.SetLocked();
					return;
				}

				var button = instance.GetComponent<Button>();

				var hero = MetaplayClient.PlayerModel.Heroes.Heroes[taskGiverInfo.Type];
				heroButton.SetHero(hero);

				button.onClick.AddListener(
					() => { HeroSelected?.Invoke(hero); }
				);
			}
		}
	}
}
