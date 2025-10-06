using Code.UI.Application;
using Code.UI.MergeBase;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Code.UI.Effects {
	[UsedImplicitly]
	public class EffectsController {
		[Inject] private MergeBoardRoot mergeBoardRoot;
		[Inject] private IslandTokenParticles islandTokenParticles;
		[Inject] private TrophyTokenParticles trophyTokenParticles;
		[Inject] private GemParticles gemParticles;
		[Inject] private GoldParticles goldParticles;
		[Inject] private EnergyParticles energyParticles;
		[Inject] private ScoreParticles scoreParticles;
		[Inject] private SignalBus signalBus;
		[Inject] private IFrameRateController frameRateController;

		public async UniTask FlyCurrencyParticles(CurrencyTypeId resourceType, int amount, int x, int y) {
			GameObject itemHandle = mergeBoardRoot.TileAt(x, y).Handle;
			Vector3 itemPosition = itemHandle.transform.position;

			await FlyCurrencyParticles(resourceType, amount, itemPosition);
		}

		public async UniTask FlyCurrencyParticles(CurrencyTypeId resourceType, int amount, Vector3 position) {
			CurrencyParticlesBase particles = null;
			if (resourceType == CurrencyTypeId.IslandTokens) {
				particles = islandTokenParticles;
			} else if (resourceType == CurrencyTypeId.TrophyTokens) {
				particles = trophyTokenParticles;
			} else if (resourceType == CurrencyTypeId.Gems) {
				particles = gemParticles;
			} else if (resourceType == CurrencyTypeId.Gold) {
				particles = goldParticles;
			} else if (resourceType == CurrencyTypeId.Energy) {
				particles = energyParticles;
			}

			if (particles != null) {
				using (frameRateController.RequestHighFPS()) {
					await particles.SpawnAt(position, amount);
				}
			}

			signalBus.Fire(new ResourcesChangedSignal(resourceType, amount));
		}

		public async UniTask FlyScoreParticles(int amount, int x, int y) {
			GameObject itemHandle = mergeBoardRoot.TileAt(x, y).Handle;

			using (frameRateController.RequestHighFPS()) {
				await scoreParticles.SpawnAt(itemHandle.transform.position, amount);
			}
		}
	}
}
