using Game.Logic;
using Metaplay.Core.Player;
using Metaplay.Unity;

public class GameOfflineServer : DefaultOfflineServer, IPlayerModelServerListener {
    public GameOfflineServer() { }
	protected override void SetSelfAsPlayerListener(IPlayerModelBase player) {
		((PlayerModel) player).ServerListener = this;
	}

	public void OnActivityEventScoreAdded(EventId @event, int level, int delta, ResourceModificationContext context) { }

	public void OnPlayerXpAdded(int delta) { }

	public void OnIslandXpAdded(IslandTypeId island, int delta) { }

	public void OnBuildingXpAdded(IslandTypeId island, int delta) { }

	public void OnHeroXpAdded(HeroTypeId hero, int delta) { }
	public void ItemMerged(ItemModel newItem, int mergeScore) { }
}
