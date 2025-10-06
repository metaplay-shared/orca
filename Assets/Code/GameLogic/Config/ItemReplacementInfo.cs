using System;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ItemReplacementInfo : IGameConfigData<ReplacementId> {
		[MetaMember(1)] public ReplacementContextId Context { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }
		[MetaMember(4)] public ChainTypeId ReplacementType { get; private set; }
		[MetaMember(5)] public int ReplacementLevel { get; private set; }

		public ReplacementId ConfigKey => new ReplacementId(Context, Type, Level);
	}

	[MetaSerializable]
	public class ReplacementContextId : StringId<ReplacementContextId> {
		public static readonly ReplacementContextId Init = FromString("Init");
		public static readonly ReplacementContextId UnlockHero = FromString("UnlockHero");
		public static readonly ReplacementContextId SeasonalEventEnd = FromString("SeasonalEventEnd");
		public static readonly ReplacementContextId NoNewHero = FromString("NoNewHero");
	}

	[MetaSerializable]
	public struct ReplacementId : IEquatable<ReplacementId> {
		[MetaMember(1)] public ReplacementContextId Context { get; private set; }
		[MetaMember(2)] public ChainTypeId Type { get; private set; }
		[MetaMember(3)] public int Level { get; private set; }

		public ReplacementId(ReplacementContextId context, ChainTypeId type, int level) {
			Context = context;
			Type = type;
			Level = level;
		}

		public bool Equals(ReplacementId other) {
			return Equals(Context, other.Context) && Equals(Type, other.Type) && Level == other.Level;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) { return false; }
			if (obj.GetType() != this.GetType()) { return false; }
			return Equals((ReplacementId) obj);
		}

		public override int GetHashCode() {
			return HashCode.Combine(Context, Type, Level);
		}

		public override string ToString() {
			return Context + ":" + Type + ":" + Level;
		}
	}
}
