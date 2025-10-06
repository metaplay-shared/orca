namespace Game.Logic {
	public struct Cost {
		public CurrencyTypeId Type { get; private set; }
		public int Amount { get; private set; }

		public Cost(CurrencyTypeId type, int amount) {
			Type = type;
			Amount = amount;
		}
	}
}
