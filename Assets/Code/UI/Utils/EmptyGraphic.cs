using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Utils {
	/// <summary>
	/// A graphic that draws nothing. Mostly used to block areas in UI without drawing any transparent graphics
	/// which would waste fill rate.
	/// </summary>
	[RequireComponent(typeof(CanvasRenderer))]
	public class EmptyGraphic : Graphic {
		protected override void OnPopulateMesh(VertexHelper vh) {
			vh.Clear();
		}
	}
}
