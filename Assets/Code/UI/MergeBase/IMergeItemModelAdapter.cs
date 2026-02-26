using Cysharp.Threading.Tasks;
using Game.Logic;

namespace Code.UI.MergeBase {
	public interface IMergeItemModelAdapter {
		object Id { get; }
		bool HasTimer { get; }
		bool IsBubble { get; }
		ChainTypeId Type { get; }
		int X { get; }
		int Y { get; }
		int Width { get; }
		int Height { get; }
		int Level { get; }
		float Progression { get; }
		ItemState State { get; }
		ItemBuildState BuildState { get; }
		bool IsUsedInTask { get; }
		bool UnderLockArea { get; }
		bool CanCollect { get; }
		bool IsMaxLevel { get; }
		string TimeLeftText { get; }
		bool QuickOpen { get; }
		bool IsHeroItemTarget { get; }
		bool IsBuilding { get; }
		bool CanRemove { get; }
		void Open();
		float GetFlightTime(float distance);
		UniTask AcknowledgeBuilding();
		void Select();
		ChainInfo GetNextLevelItemInfo();
	}
}
