using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic {
	[MetaSerializable]
	public class VipPassesModel {
		[MetaMember(1)] public MetaDictionary<VipPassId, VipPassModel> Passes { get; protected set; }

		public VipPassesModel() {
			Passes = new MetaDictionary<VipPassId, VipPassModel>();
		}

		public bool HasAnyPass => Passes.Count > 0;
		public bool HasPass(VipPassId vipPassId) => Passes.ContainsKey(vipPassId);

		public MetaDuration PassDuration(MetaTime currentTime) {
			MetaDuration duration = MetaDuration.Zero;
			foreach (VipPassModel pass in Passes.Values) {
				MetaDuration passDuration = pass.Expires - currentTime;
				duration = MetaDuration.Max(duration, passDuration);
			}

			return duration;
		}

		public bool Update(MetaTime now, IPlayerModelClientListener clientListener) {
			bool anyPassExpired = false;
			foreach (VipPassModel vipPass in Passes.Values) {
				if (now > vipPass.Expires) {
					anyPassExpired = true;
					break;
				}
			}

			// Allocate the temporary list only when a pass has actually expired (very rarely).
			if (anyPassExpired) {
				List<VipPassId> expiredPasses = new List<VipPassId>();
				foreach (VipPassModel pass in Passes.Values) {
					if (now > pass.Expires) {
						expiredPasses.Add(pass.Info.Id);
					}
				}

				foreach (VipPassId passId in expiredPasses) {
					Passes.Remove(passId);
				}
				clientListener.OnVipPassesChanged();
			}

			return anyPassExpired;
		}

		public void AddPass(InAppProductInfo productInfo, PlayerModel player) {
			VipPassId passId = productInfo.VipPassId;
			Passes.TryGetValue(passId, out VipPassModel vipPass);
			if (vipPass == null) {
				VipPassInfo vipPassInfo = player.GameConfig.VipPasses[productInfo.VipPassId];
				vipPass = new VipPassModel(vipPassInfo, player.CurrentTime + productInfo.VipPassDuration);
				Passes.Add(productInfo.VipPassId, vipPass);
			} else {
				vipPass.Extend(productInfo.VipPassDuration);
			}

			vipPass.TemporaryBuilderId = player.Builders.AddTemporaryBuilderTime(
				player.CurrentTime,
				productInfo.VipPassDuration,
				vipPass.TemporaryBuilderId
			);
			player.ClientListener.OnVipPassesChanged();
			player.ClientListener.OnBuilderStateChanged();
		}

		public int TryClaimDailyRewards(PlayerModel player) {
			int claimedRewards = 0;
			foreach (VipPassModel vipPass in Passes.Values) {
				if (vipPass.TryClaimDailyReward(player)) {
					claimedRewards++;
				}
			}

			return claimedRewards;
		}

		public int MaxEnergyBoost() {
			int boost = 0;
			foreach (VipPassModel vipPass in Passes.Values) {
				boost += vipPass.Info.MaxEnergyBoost;
			}

			return boost;
		}

		/// <summary>
		/// <c>EnergyProductionFactor</c> returns the combined energy production factor of the currently effective
		/// VIP passes. The factors are combined using addition as opposed to multiplication. That is, 10% and 20%
		/// boosts result in combined boost of 30% (instead of 32%).
		/// </summary>
		/// <returns>combined energy production factor</returns>
		public F64 EnergyProductionFactor() {
			F64 factor = F64.One;
			foreach (VipPassModel vipPass in Passes.Values) {
				if (vipPass.Info.EnergyProductionFactor > 0) {
					factor += vipPass.Info.EnergyProductionFactor - 1;
				}
			}

			return factor;
		}

		public F64 BuilderTimerFactor() {
			F64 factor = F64.One;
			foreach (VipPassModel vipPass in Passes.Values) {
				if (vipPass.Info.BuilderTimerFactor > 0) {
					factor += vipPass.Info.BuilderTimerFactor - 1;
				}
			}

			return F64.Max(factor, F64.Zero);
		}
	}

	[MetaSerializable]
	public class VipPassModel {
		[MetaMember(1)] public VipPassInfo Info { get; protected set; }
		[MetaMember(2)] public MetaTime Expires { get; protected set; }
		/// <summary>
		/// The previous day that a daily reward was claimed for this VIP pass. To handle year changes correctly
		/// the day is presented as <code>1000*year + dayOfYear</code> e.g. <c>2022014</c> for Jan 14th 2022.
		/// </summary>
		[MetaMember(3)] public int PreviousDailyRewardClaimedDay { get; protected set; }
		[MetaMember(4)] public int TemporaryBuilderId { get; internal set; }

		public VipPassModel() { }

		public VipPassModel(VipPassInfo info, MetaTime expires) {
			Info = info;
			Expires = expires;
			PreviousDailyRewardClaimedDay = -1;
			TemporaryBuilderId = -1;
		}

		public void Extend(MetaDuration duration) {
			Expires += duration;
		}

		public bool TryClaimDailyReward(PlayerModel player) {
			PlayerLocalTime localTime = player.GetCurrentLocalTime();
			DateTime localDateTime = (localTime.Time + localTime.UtcOffset).ToDateTime();
			int today = localDateTime.Year * 1000 + localDateTime.DayOfYear;
			if (today > PreviousDailyRewardClaimedDay) {
				PreviousDailyRewardClaimedDay = today;
				RewardModel reward = new RewardModel(
					Info.DailyRewardResources,
					Info.DailyRewardItems,
					ChainTypeId.LevelUpRewards,
					1,
					new RewardMetadata { Type = RewardType.VipPassDaily, VipPass = Info.Id}
				);
				player.AddReward(reward);
				return true;
			} else {
				return false;
			}
		}
	}
}
