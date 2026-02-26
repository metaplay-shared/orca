using System;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public struct IslandCoordinate : IEquatable<IslandCoordinate> {
		[MetaMember(1)] public IslandTypeId Island { get; private set; }
		[MetaMember(2)] public int X { get; private set; }
		[MetaMember(3)] public int Y { get; private set; }

		public IslandCoordinate(IslandTypeId island, int x, int y) {
			Island = island;
			X = x;
			Y = y;
		}

		public bool Equals(IslandCoordinate other) {
			return Equals(Island, other.Island) && X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) { return false; }
			if (obj.GetType() != this.GetType()) { return false; }
			return Equals((IslandCoordinate) obj);
		}

		public override int GetHashCode() {
			return HashCode.Combine(Island, X, Y);
		}

		public override string ToString() {
			return Island + ":" + X + ":" + Y;
		}
	}
}
