using Metaplay.Core.Math;
using UnityEngine;

namespace Code.UI.Utils {
	public static class MathExtensions {
		public static Vector3 ToVector3(this F64Vec2 source) {
			return new Vector3(source.X.Float, source.Y.Float);
		}

		public static Vector3 InverseY(this Vector3 vec) {
			if (vec.y > 0) {
				vec.y = -vec.y;
			}

			return vec;
		}
	}
}
