namespace Code.UI.UITween {
	public abstract class OnEnableTween : OnDemandUITween {
		protected void OnEnable() {
			CreateTween();
		}
	}
}
