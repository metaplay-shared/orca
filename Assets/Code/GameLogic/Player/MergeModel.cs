using System.Collections.Generic;
using System.Linq;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic {
	[MetaSerializable]
	public class MergeModel {
		[MetaMember(1)] public MergeInfo Info { get; private set; }
		[MetaMember(2)] public ProducerModel Energy { get; private set; }
		[MetaMember(3)] public ItemDiscovery ItemDiscovery { get; private set; }
		[MetaMember(4)] public EnergyFillModel EnergyFill { get; private set; }

		public MergeModel() { }

		public MergeModel(SharedGameConfig gameConfig, MetaTime currentTime) {
			Info = gameConfig.Merge;
			Energy = new ProducerModel();
			Energy.Reset(currentTime, Info.MaxEnergy);
			ItemDiscovery = new ItemDiscovery();
			EnergyFill = new EnergyFillModel();
		}

		public MetaDuration TimeToNext(MetaTime currentTime, F64 energyGeneratedPerHour, int maxEnergy) {
			return Energy.TimeToNext(currentTime, energyGeneratedPerHour, maxEnergy);
		}

		public void Update(
			MetaTime currentTime,
			PlayerLocalTime localTime,
			IPlayerModelClientListener clientListener,
			F64 energyGeneratedPerHour,
			int maxEnergy
		) {
			int diff = Energy.Update(currentTime, energyGeneratedPerHour, maxEnergy);
			if (diff != 0) {
				clientListener.OnResourcesModified(CurrencyTypeId.Energy, diff, ResourceModificationContext.Empty);
			}

			EnergyFill.Update(localTime);
		}
	}
}
