using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Zenject;

namespace Code.UI.Island {
	public class MapButton : ButtonHelper {
		[Inject] private UIController uiController;

		protected override void OnClick() {
			uiController.LeaveIsland(default).Forget();
		}
	}
}
