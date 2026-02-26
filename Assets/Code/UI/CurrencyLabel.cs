using System;
using Code.UI.Application;
using Code.UI.Utils;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;
using Zenject;

namespace Code.UI {
	public class CurrencyLabel : MonoBehaviour {
		[SerializeField] private TMP_Text Text;

		private SignalBus signalBus;
		private CurrencyTypeId CurrencyType { get; set; } = CurrencyTypeId.None;
		private int Amount { get; set; }
		private bool showOwned;

		public TMP_Text TextComponent => Text;

		private void OnEnable() {
			signalBus.Subscribe<ResourcesChangedSignal>(UpdateColor);
			UpdateColor();
		}

		private void OnDisable() {
			signalBus.TryUnsubscribe<ResourcesChangedSignal>(UpdateColor);
		}

		[Inject]
		private void Setup(SignalBus signalBus) {
			this.signalBus = signalBus;
		}

		private void UpdateColor() {
			if (!CurrencyType.WalletResource) {
				return;
			}

			if (showOwned) {
				int owned = MetaplayClient.PlayerModel.Wallet.Currency(CurrencyType).Value;
				Text.text = $"<sprite name={CurrencyType}> {owned}/{Amount.ToString()}";
			}

			if (MetaplayClient.PlayerModel.Wallet.EnoughCurrency(CurrencyType, Amount)) {
				Text.color = Color.white;
			} else {
				Text.color = Color.red;
			}
		}

		public void Set(CurrencyTypeId currencyType, int amount, bool showOwned = false) {
			this.showOwned = showOwned;
			CurrencyType = currencyType;
			Amount = amount;
			if (amount == 0) {
				Text.text = Localizer.Localize("Cost.Free");
			} else {
				Text.text = $"<sprite name={currencyType.Value}> {amount.ToString()}";
			}

			UpdateColor();
		}

		public void Set(string text) {
			CurrencyType = CurrencyTypeId.None;
			Text.text = text;
			Text.color = Color.white;
		}
	}
}
