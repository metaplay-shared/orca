using System;

namespace Game.Logic {
	public class Coordinates {
		public int X { get; private set; }
		public int Y { get; private set; }

		public Coordinates(int x, int y) {
			X = x;
			Y = y;
		}

		protected bool Equals(Coordinates other) {
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Coordinates)obj);
		}

		public override int GetHashCode() {
			return HashCode.Combine(X, Y);
		}

		public override string ToString() {
			return $"({X},{Y})";
		}
	}
}
