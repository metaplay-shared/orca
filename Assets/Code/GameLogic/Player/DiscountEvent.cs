using System.Runtime.Serialization;
using Metaplay.Core;
using Metaplay.Core.Activables;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic {
	[MetaSerializableDerived(1)]
	public class DiscountEventsModel : MetaActivableSet<EventId, DiscountEventInfo,
		DiscountEventModel> {
		protected override DiscountEventModel CreateActivableState(
			DiscountEventInfo info,
			IPlayerModelBase player
		) {
			return new DiscountEventModel(info);
		}

		public DiscountEventModel SubEnsureHasState(DiscountEventInfo info, IPlayerModelBase player) {
			return EnsureHasState(info, player);
		}
	}

	[MetaSerializableDerived(1)]
	public class DiscountEventModel : MetaActivableState<EventId, DiscountEventInfo>, IEventModel {
		[MetaMember(1)] public sealed override EventId ActivableId { get; protected set; }
		[MetaMember(2)] public MetaTime AdSeenTime { get; protected set; }

		[IgnoreDataMember] public DiscountEventInfo Info => ActivableInfo;
		[IgnoreDataMember] public IMetaActivableConfigData<EventId> EventInfo => ActivableInfo;
		[IgnoreDataMember] public string Icon => Info.Icon;
		[IgnoreDataMember] public MetaActivableParams MetaActivableParams => ActivableInfo.ActivableParams;
		[IgnoreDataMember] public int VisualizationOrder => ActivableInfo.VisualizationOrder;
		[IgnoreDataMember] public EventAdMode AdMode => ActivableInfo.AdMode;

		public bool AdSeen => AdSeenTime > MetaTime.Epoch;

		public DiscountEventModel() { }
		public DiscountEventModel(DiscountEventInfo info) : base(info) { }

		protected override void OnStartedActivation(IPlayerModelBase player) {
			((PlayerModel)player).ClientListener.OnEventStateChanged(ActivableId);
		}

		protected override void Finalize(IPlayerModelBase player) {
			if (Info.ReshowAd) {
				AdSeenTime = MetaTime.Epoch;
			}
			((PlayerModel)player).ClientListener.OnEventStateChanged(ActivableId);
		}

		public void MarkAdSeen(MetaTime time) {
			AdSeenTime = time;
		}
	}
}
