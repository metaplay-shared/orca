using Cysharp.Threading.Tasks;
using Game.Logic;
using UnityEngine;
using Zenject;

namespace Code.UI.Map {
	public class Island : MonoBehaviour {
		[Inject] private CameraControls cameraControls;
		[Inject] private UIController uiController;
		public IslandModel Model { get; set; }
	}
}
