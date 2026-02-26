using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class LogbookTaskInfo : IGameConfigData<LogbookTaskId> {
		[MetaMember(1)] public LogbookTaskId Id { get; private set; }
		[MetaMember(2)] public LogbookChapterId Chapter { get; private set; }
		[MetaMember(3)] public LogbookTaskType Type { get; private set; }
		[MetaMember(4)] public LevelId<ChainTypeId> Item { get; private set; }
		[MetaMember(5)] public bool UseValueAsIncrement { get; private set; }
		[MetaMember(6)] public IslandTypeId Island { get; private set; }
		[MetaMember(7)] public HeroTypeId Hero { get; private set; }
		[MetaMember(8)] public int Count { get; private set; }
		[MetaMember(9)] public ResourceInfo Reward { get; private set; }
		[MetaMember(10)] public List<LogbookTaskId> Dependencies { get; private set; }
		[MetaMember(11)] public string Icon { get; private set; }
		[MetaMember(12)] public string TitleKey { get; private set; }
		[MetaMember(13)] public List<LogbookTaskOperationInfo> Operations { get; private set; }

		public LogbookTaskId ConfigKey => Id;
	}

	[MetaSerializable]
	public class LogbookTaskId : StringId<LogbookTaskId> {}

	[MetaSerializable]
	public class LogbookTaskType : StringId<LogbookTaskType> {
		// Discover an item (Count should always be configured as 1)
		public static readonly LogbookTaskType ItemDiscovery = FromString("ItemDiscovery");
		// Create (or discover) items
		public static readonly LogbookTaskType ItemCount = FromString("ItemCount");
		// Complete all daily tasks
		public static readonly LogbookTaskType DailyTasksComplete = FromString("DailyTasksComplete");
		// Complete any hero task
		public static readonly LogbookTaskType HeroTasks = FromString("HeroTasks");
		// Merge any two items
		public static readonly LogbookTaskType Merge = FromString("Merge");
		// Complete a single daily task
		public static readonly LogbookTaskType DailyTask = FromString("DailyTask");
		// Use permanent (non-disposable) mine
		public static readonly LogbookTaskType UseMine = FromString("UseMine");
		public static readonly LogbookTaskType OpenChest = FromString("OpenChest");
		public static readonly LogbookTaskType RepairMine = FromString("RepairMine");
		public static readonly LogbookTaskType UseBuilder = FromString("UseBuilder");
		public static readonly LogbookTaskType UseBooster = FromString("UseBooster");
		// Unlock specific island (Island need to be configured)
		public static readonly LogbookTaskType UnlockIsland = FromString("UnlockIsland");
		// Collect item
		// - configuring Item level as 0 means "any level"
		// - configuring Item type as None means "any type"
		public static readonly LogbookTaskType CollectItem = FromString("CollectItem");
	}

	[MetaSerializable]
	public abstract class LogbookTaskOperationInfo {
		[MetaMember(100)] public LogbookTaskOperationType Type { get; private set; }

		public LogbookTaskOperationInfo() {}

		public LogbookTaskOperationInfo(LogbookTaskOperationType type) {
			Type = type;
		}
	}

	[MetaSerializableDerived(1)]
	public class OpenIslandOperationInfo : LogbookTaskOperationInfo {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }

		public OpenIslandOperationInfo() { }

		public OpenIslandOperationInfo(IslandTypeId island) : base(LogbookTaskOperationType.OpenIsland) {
			Island = island;
		}
	}

	[MetaSerializableDerived(2)]
	public class FocusIslandOperationInfo : LogbookTaskOperationInfo {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		public FocusIslandOperationInfo() { }

		public FocusIslandOperationInfo(IslandTypeId island) : base(LogbookTaskOperationType.FocusIsland) {
			Island = island;
		}
	}

	[MetaSerializableDerived(3)]
	public class SelectItemOperationInfo : LogbookTaskOperationInfo {
		[MetaMember(1)] public LevelId<ChainTypeId> Item { get; private set; }

		public SelectItemOperationInfo() { }

		public SelectItemOperationInfo(LevelId<ChainTypeId> item) : base(LogbookTaskOperationType.SelectItem) {
			Item = item;
		}
	}

	[MetaSerializableDerived(4)]
	public class OpenDailyTasksOperationInfo : LogbookTaskOperationInfo {
		public OpenDailyTasksOperationInfo() : base(LogbookTaskOperationType.OpenDailyTasks) { }
	}

	[MetaSerializable]
	public class LogbookTaskOperationType : StringId<LogbookTaskOperationType> {
		public static readonly LogbookTaskOperationType OpenIsland = FromString("OpenIsland");
		public static readonly LogbookTaskOperationType FocusIsland = FromString("FocusIsland");
		public static readonly LogbookTaskOperationType SelectItem = FromString("SelectItem");
		public static readonly LogbookTaskOperationType OpenDailyTasks = FromString("OpenDailyTasks");
	}
}
