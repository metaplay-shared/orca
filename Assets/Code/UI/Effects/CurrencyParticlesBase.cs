using System.Collections.Generic;
using Code.UI.Effects.Signals;
using Game.Logic;
using UnityEngine;
using Zenject;

namespace Code.UI.Effects {
	public abstract class CurrencyParticlesBase : EffectSpawnerBase {
		protected abstract CurrencyTypeId Type { get; }

		[Inject] private SignalBus signalBus;

		/**
		 * Forces particles to be destroyed when they collide with their designated collider. Also notifies the app that
		 * particle has been destroyed so that the resource counters can be updated in realtime.
		 *
		 * Setup:
		 * - Create a gameobject with force field that sucks the particles, 2D collider and assign it a layer (coins, gems or whatever the currency is)
		 * - Create a particle system under ParticleEffects prefab.
		 * - For the system, enable Collision, set "Collides With" to the layer you created earlier
		 * - For the system, enable Trigger. Drag the collider to the gameobject slot and set Enter to Callback
		 * - Install the emitter via EffectsInstaller
		 */
		private void OnParticleTrigger() {
			ParticleSystem particleSystem = GetComponent<ParticleSystem>();

			List<ParticleSystem.Particle> triggeredParticles = new List<ParticleSystem.Particle>();
			int numberCollided = particleSystem.GetTriggerParticles(
				ParticleSystemTriggerEventType.Enter,
				triggeredParticles
			);

			for (int i = 0; i < numberCollided; i++) {
				ParticleSystem.Particle p = triggeredParticles[i];
				p.remainingLifetime = 0;

				/*
				 ToDo: Implement concept of particle value. For example if player is awarded with million gold,
				 we rather show just thousand particles but each particle increment the counter by 1000,
				 value of one particle being 1000 instead of 1.
				 */
				signalBus.Fire(new ParticleDestroyedSignal(Type, 1));
			}
		}
	}
}
