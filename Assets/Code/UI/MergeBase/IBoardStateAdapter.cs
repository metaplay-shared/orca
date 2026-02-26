namespace Code.UI.MergeBase {
	public interface IBoardStateAdapter {
		IMergeItemModelAdapter[] MergeItems { get; }
		string Identifier { get; }
		bool CanMoveFrom(int x, int y);
		bool CanMoveTo(int fromX, int fromY, int toX, int toY);
		bool CanMergeTo(int fromX, int fromY, int toX, int toY);
		void MoveItem(int fromX, int fromY, int toX, int toY);
	}
}
