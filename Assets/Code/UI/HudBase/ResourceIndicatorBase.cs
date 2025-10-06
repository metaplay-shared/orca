using Code.UI.Application.Signals;
using Code.UI.Effects;
using Code.UI.Effects.Signals;
using DG.Tweening;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.HudBase {
	public abstract class ResourceIndicatorBase : MonoBehaviour {
		protected abstract int ResourceAmount { get; }
		protected abstract CurrencyTypeId Type { get; }

		[SerializeField] protected TMP_Text ResourceAmountText;
		[SerializeField] private Image ResourceIcon;

		[Header("Animation")]
		[SerializeField] private Vector3 TextPunchScale = new Vector3(0.5f, 0.5f);
		[SerializeField] private Vector3 IconPunchScale = new Vector3(0.5f, 0.5f);
		[SerializeField] private float Duration = 0.35f;

		[Inject] protected SignalBus signalBus;

		public virtual void OnClick() { }

		protected virtual void Start() {
			ResourceAmountText.text = ResourceAmount.ToString();
		}

		protected virtual void OnEnable() {
			signalBus.Subscribe<ResourcesChangedSignal>(OnResourceUpdated);
			signalBus.Subscribe<ParticleDestroyedSignal>(OnParticleDestroyed);
		}

		protected virtual void OnDisable() {
			signalBus.Unsubscribe<ResourcesChangedSignal>(OnResourceUpdated);
			signalBus.Unsubscribe<ParticleDestroyedSignal>(OnParticleDestroyed);
		}

		private void OnParticleDestroyed(ParticleDestroyedSignal signal) {
			if (signal.Type != Type) {
				return;
			}

			int currentValue = int.Parse(ResourceAmountText.text);
			int newValue = currentValue + signal.Value;
			ResourceAmountText.text = newValue.ToString();
			PlayOnHitAnimation();
		}

		private void OnResourceUpdated(ResourcesChangedSignal signal) {
			if (signal.ResourceType != Type) {
				return;
			}

			ResourceAmountText.text = ResourceAmount.ToString();
		}

		protected virtual void PlayOnHitAnimation() {
			DOTween.Complete(ResourceAmountText.transform);
			DOTween.Complete(ResourceIcon.transform);
			ResourceAmountText.transform.DOPunchScale(TextPunchScale, Duration);
			ResourceIcon.transform.DOPunchScale(IconPunchScale, Duration);
		}
	}
}
