namespace Game.Logic {
	public class ResourceModificationContext {
		public static readonly ResourceModificationContext Empty = new ResourceModificationContext();
	}

	public class ShopResourceContext : ResourceModificationContext {
		public ShopItemId Item { get; private set; }

		public ShopResourceContext(ShopItemId item) {
			Item = item;
		}
	}

	public class MarketResourceContext : ResourceModificationContext {
		public ShopCategoryId Category { get; private set; }
		public int Index { get; private set; }

		public MarketResourceContext(ShopCategoryId category, int index) {
			Category = category;
			Index = index;
		}
	}

	public class FlashSaleResourceContext : ResourceModificationContext {
		public ShopCategoryId Category { get; private set; }
		public int Index { get; private set; }

		public FlashSaleResourceContext(ShopCategoryId category, int index) {
			Category = category;
			Index = index;
		}
	}

	public class MergeBoardResourceContext : ResourceModificationContext {
		public int X { get; private set; }
		public int Y { get; private set; }

		public MergeBoardResourceContext(int x, int y) {
			X = x;
			Y = y;
		}
	}

	public class DiscoveryResourceContext : ResourceModificationContext {
		public ChainTypeId Type { get; private set; }
		public int Level { get; private set; }

		public DiscoveryResourceContext(ChainTypeId type, int level) {
			Type = type;
			Level = level;
		}
	}

	public class IslanderTaskResourceContext : ResourceModificationContext {
		public IslanderId Islander { get; private set; }

		public IslanderTaskResourceContext(IslanderId islander) {
			Islander = islander;
		}
	}

	public class HeroTaskResourceContext : ResourceModificationContext {
		public HeroTypeId Hero { get; private set; }

		public HeroTaskResourceContext(HeroTypeId hero) {
			Hero = hero;
		}
	}

	public class DailyTaskResourceContext : ResourceModificationContext {
		public DailyTaskInfo TaskInfo { get; private set; }
		// Task's index within the set of tasks for the particular day.
		public int Slot { get; private set; }

		public DailyTaskResourceContext(DailyTaskInfo taskInfo, int slot) {
			TaskInfo = TaskInfo;
			Slot = slot;
		}
	}

	public class MailResourceContext : ResourceModificationContext {
		public MailResourceContext() { }
	}
}
