using Metaplay.Core;
using Metaplay.Core.Activables;

namespace Game.Logic {

	public interface IEventModel {
		EventId EventId => EventInfo.ConfigKey;
		IMetaActivableConfigData<EventId> EventInfo { get; }
		bool AdSeen { get; }
		void MarkAdSeen(MetaTime time);
		string Icon { get; }
		MetaActivableParams MetaActivableParams { get; }
		int VisualizationOrder { get; }
		EventAdMode AdMode { get; }

		MetaActivableVisibleStatus Status(PlayerModel player) {
			return player.Status(this);
		}
	}
}
