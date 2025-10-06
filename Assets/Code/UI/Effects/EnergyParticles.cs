using Game.Logic;

namespace Code.UI.Effects {
	public class EnergyParticles : CurrencyParticlesBase {
		protected override CurrencyTypeId Type => CurrencyTypeId.Energy;
	}
}
