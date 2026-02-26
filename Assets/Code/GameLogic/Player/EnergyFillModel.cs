using System;
using Metaplay.Core;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic {
	[MetaSerializable]
	public class EnergyFillModel {
		[MetaMember(1)] public int CurrentIndex { get; private set; } = 1;
		[MetaMember(2)] public MetaTime LastReset { get; private set; } = MetaTime.Epoch;

		public void Update(PlayerLocalTime localTime) {
			if (localTime.Time - LastReset >= MetaDuration.FromDays(1)) {
				DateTime dateTime = (localTime.Time + localTime.UtcOffset).ToDateTime();
				LastReset = MetaTime.FromDateTime(dateTime.Date);
				CurrentIndex = 1;
			}
		}

		public void UpdateCurrentIndex(SharedGameConfig gameConfig) {
			CurrentIndex = Math.Min(CurrentIndex + 1, gameConfig.EnergyCosts.Count);
		}

		public EnergyCostInfo EnergyCost(SharedGameConfig gameConfig) {
			if (!gameConfig.EnergyCosts.ContainsKey(CurrentIndex)) {
				return gameConfig.EnergyCosts[gameConfig.EnergyCosts.Count];
			}

			return gameConfig.EnergyCosts[CurrentIndex];
		}
	}
}
