using Code.UI;
using Code.UI.Core;
using Code.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Core.InAppPurchase;
using Metaplay.Unity.DefaultIntegration;

namespace Code.Purchasing {
	public interface IPurchasingFlowController {
		public UniTask<bool> TrySpendGemsAsync(int amount, CancellationToken ct);
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class PurchasingFlowController : IPurchasingFlowController {
		private readonly IUIRootController uiRootController;

		public PurchasingFlowController(IUIRootController uiRootController) {
			this.uiRootController = uiRootController;
		}

		public async UniTask<bool> TrySpendGemsAsync(int amount, CancellationToken ct) {
			CurrencyTypeId currencyType = CurrencyTypeId.Gems;
			if (amount == 0) {
				return true;
			}

			int balance = GetBalanceForCurrency(currencyType);
			bool askedPurchase = false;

			if (balance < amount) {
				OfferPopupHandle handle = uiRootController.ShowUI<OfferPopup, OfferPopupHandle>(
					new OfferPopupHandle(ResolveBundle(amount), false),
					ct
				);

				await handle.OnComplete;
				askedPurchase = true;
			}

			balance = GetBalanceForCurrency(currencyType);
			if (balance < amount) {
				return false;
			}

			if (!askedPurchase && MetaplayClient.PlayerModel.GameConfig.Global.ConfirmGemSpend) {
				ConfirmationPopupHandle handle = uiRootController.ShowUI<ConfirmationPopup, ConfirmationPopupHandle>(
					new ConfirmationPopupHandle(
						Localizer.Localize("Info.WantToSpendGems"),
						Localizer.Localize("Info.AboutToSpendGems", amount),
						ConfirmationPopupHandle.ConfirmationPopupType.YesNo
					),
					ct
				);

				ConfirmationPopupResult result = await handle.OnCompleteWithResult;

				if (result.Response != ConfirmationPopupResponse.Yes) {
					return false;
				}
			}

			return true;
		}

		private InAppProductId ResolveBundle(int amount) {
			int requiredAmount = amount - MetaplayClient.PlayerModel.Wallet.Gems.Value;

			InAppProductId minProductId = MetaplayClient.PlayerModel.GameConfig.Global.MinOfferedGemProduct;
			InAppProductInfo minProduct = MetaplayClient.PlayerModel.GameConfig.InAppProducts[minProductId];
			int minProductGems = minProduct.Resources.Find(r => r.Type == CurrencyTypeId.Gems).Amount;

			List<InAppProductInfo> products = MetaplayClient.PlayerModel.GameConfig.InAppProducts.Values
				.Where(p => p.Resources.Count > 0 && p.Resources.Exists(r => r.Type == CurrencyTypeId.Gems))
				.OrderBy(p => p.Resources.Find(r => r.Type == CurrencyTypeId.Gems).Amount)
				.ToList();

			foreach (InAppProductInfo product in products) {
				ResourceInfo resources = product.Resources.Find(r => r.Type == CurrencyTypeId.Gems);
				if (resources.Amount >= minProductGems &&
					resources.Amount >= requiredAmount) {
					return product.ConfigKey;
				}
			}

			return products[^1].ConfigKey;
		}

		private int GetBalanceForCurrency(CurrencyTypeId currencyType) {
			if (currencyType == CurrencyTypeId.Gems) {
				return MetaplayClient.PlayerModel.Wallet.Gems.Value;
			}

			if (currencyType == CurrencyTypeId.Gold) {
				return MetaplayClient.PlayerModel.Wallet.Gold.Value;
			}

			throw new ArgumentException("Not supported currency type");
		}
	}
}
