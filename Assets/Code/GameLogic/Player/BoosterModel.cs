using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class BoosterModel {
		[MetaMember(1)] public BoosterInfo Info { get; set; }
		[MetaMember(2)] public MetaDuration BuildTime { get; set; }

		public BoosterModel() { }

		public BoosterModel(BoosterInfo info) {
			Info = info;
			BuildTime = info.InitialBuildTime;
		}

		public bool IsWildcard => Info.MaxWildcardLevel > 0;
		public bool IsBuilder => Info.InitialBuildTime > MetaDuration.Zero;
	}
}
