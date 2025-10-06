using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace Code.UI.Application {
	[UsedImplicitly]
	public class FrameRateInstaller : Installer {
		public override void InstallBindings() {
			Container.BindInterfacesTo<FrameRateController>().AsSingle();
			#if UNITY_EDITOR || DEVELOPMENT_BUILD
			Container.BindInterfacesTo<FrameRateDebugger>().FromMethod(CreateFrameRateDebugger).AsSingle();
			#endif
		}

		private FrameRateDebugger CreateFrameRateDebugger(InjectContext ctx) {
			GameObject go = new ("FrameRateDebugger");
			Object.DontDestroyOnLoad(go);
			FrameRateDebugger component = go.AddComponent<FrameRateDebugger>();
			ctx.Container.Inject(component);
			return component;
		}
	}
}
