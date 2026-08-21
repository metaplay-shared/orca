using System;
using System.Collections.Generic;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {

	[MetaSerializable]
	public class DailyTaskSlotAlternativesInfo : IGameConfigData<LevelId<DailyTaskSetId>> {
		[MetaMember(1)] public DailyTaskSetId DailyTaskSetId { get; private set; }
		[MetaMember(2)] public int Slot { get; private set; }
		[MetaMember(3)] public List<DailyTaskInfo> Tasks { get; private set; }

		public LevelId<DailyTaskSetId> ConfigKey => new LevelId<DailyTaskSetId>(DailyTaskSetId, Slot);
	}
}
