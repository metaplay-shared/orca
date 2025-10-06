using Code.UI.Effects.Signals;
using UnityEngine;
using Zenject;

namespace Code.UI.Effects {
	public class EffectInstaller : MonoInstaller {
		[SerializeField] private CloudParticles CloudParticles;
		[SerializeField] private IslandTokenParticles IslandTokenParticles;
		[SerializeField] private TrophyTokenParticles TrophyTokenParticles;
		[SerializeField] private GemParticles GemParticles;
		[SerializeField] private GoldParticles GoldParticles;
		[SerializeField] private EnergyParticles EnergyParticles;
		[SerializeField] private ScoreParticles ScoreParticles;

		public override void InstallBindings() {
			Container.BindInstance(CloudParticles).AsSingle();
			Container.BindInstance(IslandTokenParticles).AsSingle();
			Container.BindInstance(TrophyTokenParticles).AsSingle();
			Container.BindInstance(GemParticles).AsSingle();
			Container.BindInstance(GoldParticles).AsSingle();
			Container.BindInstance(EnergyParticles).AsSingle();
			Container.BindInstance(ScoreParticles).AsSingle();
			Container.Bind<EffectsController>().AsSingle();

			Container.DeclareSignal<ParticleDestroyedSignal>().OptionalSubscriber();
		}
	}
}
