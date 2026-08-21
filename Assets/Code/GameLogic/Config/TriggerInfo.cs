using Metaplay.Core.Config;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class TriggerInfo : IGameConfigData<TriggerId> {
		[MetaMember(1)] public TriggerId Trigger { get; private set; }
		[MetaMember(2)] public FeatureTypeId Feature { get; private set; }
		[MetaMember(3)] public DialogueId Dialogue { get; private set; }
		[MetaMember(4)] public string HighlightElement { get; private set; }
		[MetaMember(5)] public LevelId<ChainTypeId> MergeHint { get; private set; }
		[MetaMember(6)] public IslandTypeId GoToIsland { get; private set; }
		[MetaMember(7)] public IslandTypeId HighlightIsland { get; private set; }
		[MetaMember(8)] public LevelId<ChainTypeId> HighlightItem { get; private set; }
		[MetaMember(9)] public LevelId<ChainTypeId> PointItem { get; private set; }
		[MetaMember(10)] public IslandTypeId PointIsland { get; private set; }
		[MetaMember(11)] public InAppProductId Offer { get; private set; }
		[MetaMember(12)] public ChainTypeId UnlockResourceItem { get; private set; }
		[MetaMember(13)] public ChainTypeId UnlockResourceCreator { get; private set; }

		public TriggerId ConfigKey => Trigger;
	}
}
