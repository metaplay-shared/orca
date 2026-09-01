using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI.Effects {
	public class EffectSpawnerBase : MonoBehaviour {
		private ParticleSystem particles;

		private void Awake() {
			particles = GetComponent<ParticleSystem>();
		}

		public async UniTask SpawnAt(Vector3 position, int amount) {
			transform.position = position;
			particles.Emit(amount);

			await UniTask.WaitUntil(() => particles.particleCount == 0);
		}
	}
}
