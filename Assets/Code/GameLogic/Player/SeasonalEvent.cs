using System;
using System.Runtime.Serialization;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic {
	[MetaSerializableDerived(4)]
	public class SeasonalEventModel : MetaActivableState<EventId, SeasonalEventInfo>, IEventModel {
		[MetaMember(1)] public sealed override EventId ActivableId { get; protected set; }
		[MetaMember(2)] public MetaTime AdSeenTime { get; protected set; }

		[IgnoreDataMember] public SeasonalEventInfo Info => ActivableInfo;
		[IgnoreDataMember] public IMetaActivableConfigData<EventId> EventInfo => ActivableInfo;
		[IgnoreDataMember] public string Icon => Info.Icon;
		[IgnoreDataMember] public MetaActivableParams MetaActivableParams => ActivableInfo.ActivableParams;
		[IgnoreDataMember] public int VisualizationOrder => ActivableInfo.VisualizationOrder;
		[IgnoreDataMember] public EventAdMode AdMode => ActivableInfo.AdMode;

		public SeasonalEventModel() { }
		public SeasonalEventModel(SeasonalEventInfo info) : base(info) { }

		public void MarkAdSeen(MetaTime time) {
			AdSeenTime = time;
		}

		public bool AdSeen => AdSeenTime > MetaTime.Epoch;

		protected override void OnStartedActivation(IPlayerModelBase player) {
			((PlayerModel)player).ClientListener.OnEventStateChanged(ActivableId);
		}

		protected override void Finalize(IPlayerModelBase player) {
			if (Info.ReshowAd) {
				AdSeenTime = MetaTime.Epoch;
			}

			PlayerModel playerModel = (PlayerModel)player;
			foreach (IslandModel islandModel in playerModel.Islands.Values) {
				MergeBoardModel board = islandModel.MergeBoard;
				if (board != null) {
					board.ReplaceItems(
						playerModel.GameConfig,
						player.CurrentTime,
						ReplacementContextId.SeasonalEventEnd,
						playerModel.ClientListener
					);
				}
			}

			playerModel.ClientListener.OnEventStateChanged(ActivableId);
		}
	}
}
