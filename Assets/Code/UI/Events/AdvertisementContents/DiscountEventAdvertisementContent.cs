using System.Threading;
using Code.UI.AssetManagement;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Events.AdvertisementContents {
	public class DiscountEventAdvertisementContent : EventAdvertisementContentBase<DiscountEventInfo> {
		[SerializeField] private TMP_Text Text;
		[SerializeField] private Image Icon;

		[Inject] private AddressableManager addressableManager;

		protected override void Setup(DiscountEventInfo contentInfo, bool active) {
			if (contentInfo.DiscountEventType == DiscountEventType.Energy) {
				int percentage = (int)((contentInfo.EnergyProductionFactor.Float - 1.0f) * 100);
				Text.text = Localizer.Localize("Event.Info.Discount.Energy", percentage);
			} else if (contentInfo.DiscountEventType == DiscountEventType.BuilderTimer) {
				int percentage = (int)((1.0 - contentInfo.BuilderTimerFactor.Float) * 100);
				Text.text = Localizer.Localize("Event.Info.Discount.BuilderTimer", percentage);
			} else if (contentInfo.DiscountEventType == DiscountEventType.BuilderTimerGold) {
				Text.text = Localizer.Localize("Event.Info.Discount.BuilderTimerGold");
			}

			SetupIcons(contentInfo, this.GetCancellationTokenOnDestroy()).Forget();
		}

		private async UniTask SetupIcons(DiscountEventInfo discountEventInfo, CancellationToken ct) {
			Icon.sprite = await addressableManager.GetLazy<Sprite>(discountEventInfo.Icon)
				.AttachExternalCancellation(ct);
		}
	}
}
