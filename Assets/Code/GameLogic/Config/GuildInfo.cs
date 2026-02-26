using System.Collections.Generic;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class GuildInfo : GameConfigKeyValue<GuildInfo> {
		[MetaMember(1)] public int                      NumGoldPerSoldPoke    = 50;
		[MetaMember(2)] public int                      VanityCostNumGold     = 100;
		[MetaMember(3)] public int[]                    VanityRankThresholds  = { 5, 10, 15 };
		[MetaMember(4)] public int[]                    VanityRankRewardGold  = { 50, 50, 50 };
		[MetaMember(5)] public int[]                    VanityRankRewardGems  = { 0, 100, 100 };
	}
}
