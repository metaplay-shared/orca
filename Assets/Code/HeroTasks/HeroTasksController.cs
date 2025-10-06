using Code.UI.Core;
using Code.UI.InfoMessage.Signals;
using Code.UI.Merge.Hero;
using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity.DefaultIntegration;
using Orca.Common;
using System.Collections.Generic;
using System.Threading;
using Zenject;

namespace Code.HeroTasks {
	public interface IHeroTasksFlowController {
		UniTask Run(ChainTypeId infoType, CancellationToken ct);
	}

	[UsedImplicitly]
	public class HeroTasksController : IHeroTasksFlowController {
		private readonly SignalBus signalBus;
		private readonly IUIRootController uiRootController;
		private readonly IUIRootProvider uiRootProvider;

		public HeroTasksController(
			IUIRootController uiRootController,
			IUIRootProvider uiRootProvider,
			SignalBus signalBus
		) {
			this.uiRootController = uiRootController;
			this.uiRootProvider = uiRootProvider;
			this.signalBus = signalBus;
		}

		public async UniTask Run(ChainTypeId chainTypeId, CancellationToken ct) {
			List<HeroModel> heroes = MetaplayClient.PlayerModel.Heroes.HeroesInBuilding(chainTypeId);
			if (heroes.Count > 0) {
				Option<string> prefabName = GetPrefabName(chainTypeId);
				await uiRootController.ShowUI<HeroPopup, HeroPopupPayload>(
					new HeroPopupPayload(chainTypeId),
					ct,
					prefabName
				).OnComplete;
			} else {
				signalBus.Fire(new InfoMessageSignal(Localizer.Localize("Info.HeroRequired")));
			}
		}

		private Option<string> GetPrefabName(ChainTypeId chainTypeId) {
			string variantName = GetVariantAddress(chainTypeId);
			bool hasVariant = uiRootProvider.UIRootExists(variantName);
			return hasVariant
				? variantName
				: default(Option<string>);
		}

		private string GetVariantAddress(ChainTypeId chainTypeId) {
			return $"{nameof(HeroPopup)}_{chainTypeId.Value}";
		}
	}
}
