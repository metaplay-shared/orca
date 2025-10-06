using Code.UI.Core.UIBlock.UI;
using JetBrains.Annotations;
using Orca.Common;
using UnityEngine;

namespace Code.UI.Core.UIBlock {
	public interface IUIBlockOverlayProvider {
		Option<IUIBlockOverlay> LoadUIOverlay();
	}
	
	[UsedImplicitly]
	public class UIBlockOverlayProvider : IUIBlockOverlayProvider {
		public Option<IUIBlockOverlay> LoadUIOverlay(){
			return Resources.Load("Prefabs/UI/UIBlockOverlay").ToOption()
				.Map(o => o as GameObject)
				.Map(Object.Instantiate)
				.Map(go => go.GetComponent<UIBlockOverlay>() as IUIBlockOverlay);
		}
	}
}
