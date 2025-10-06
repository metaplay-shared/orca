using Code.UI.InfoMessage;
using Code.UI.InfoMessage.Signals;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Code.UI.Tasks {
	public class CompleteTaskButton : ButtonHelper {
		[SerializeField] private Sprite CanUse;
		[SerializeField] private Sprite CannotUse;
		
		private HeroTypeId hero;
		private IslandTypeId islandType;
		private IslanderId islander;

		private bool CanCompleteTask {
			get {
				HeroTaskModel task = MetaplayClient.PlayerModel.Heroes.Heroes[hero].CurrentTask;
				if (task == null) {
					return false;
				}
				return MetaplayClient.PlayerModel.Inventory.HasEnoughResources(task.Info);
			}
		}

		protected override void OnClick() {
			if (hero != null) {
				if (CanCompleteTask) {
					MetaplayClient.PlayerContext.ExecuteAction(new PlayerFulfillHeroTask(hero));
				} else {
					signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.NotEnoughResources")));
				}

			} else if (islander != null &&
						islandType != null) {
				MetaplayClient.PlayerContext.ExecuteAction(new PlayerFulfillIslandTask(islandType, islander));
			}
		}

		public void Setup(HeroTypeId heroType) {
			hero = heroType;

			if (!CanCompleteTask) {
				GetComponent<Button>().image.sprite = CannotUse;
			} else {
				GetComponent<Button>().image.sprite = CanUse;
			}
		}

		public void Setup(IslandTypeId islandTypeId, IslanderId islanderId) {
			islander = islanderId;
			islandType = islandTypeId;
		}
	}
}
