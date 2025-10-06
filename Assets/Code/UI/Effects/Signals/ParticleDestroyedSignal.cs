using Game.Logic;

namespace Code.UI.Effects.Signals {
	public class ParticleDestroyedSignal {
		public CurrencyTypeId Type { get; }
		public int Value { get; }

		public ParticleDestroyedSignal(CurrencyTypeId type, int value) {
			Type = type;
			Value = value;
		}
	}
}
