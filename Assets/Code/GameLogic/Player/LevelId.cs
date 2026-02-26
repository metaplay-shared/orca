using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Model;

namespace Game.Logic {

	[MetaSerializable]
	public struct LevelId<T> : IEquatable<LevelId<T>> where T : StringId<T>, new() {
		[MetaMember(1)] public T Type { get; private set; }
		[MetaMember(2)] public int Level { get; private set; }

		public LevelId(T type, int level) {
			Type = type;
			Level = level;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((LevelId<T>) obj);
		}

		public bool Equals(LevelId<T> other) {
			return Type == other.Type && Level == other.Level;
		}

		public override int GetHashCode() {
			unchecked {
				return (EqualityComparer<T>.Default.GetHashCode(Type) * 397) ^ Level;
			}
		}

		public override string ToString() {
			return Type + ":" + Level;
		}
	}
}
