using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Code.UI.Core;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI.Builders {
	public class BuilderPopupPayload : UIHandleBase { }

	public class BuilderPopup : UIRootBase<BuilderPopupPayload> {
		[SerializeField] protected Button CloseButton;
		[SerializeField] private RectTransform Container;
		[SerializeField] private BuilderItem PrefabBuilderItem;

		[Inject] private DiContainer container;

		protected override void Init() {
			Clear();

			List<BuilderModel> builders = MetaplayClient.PlayerModel.Builders.Permanent.Values.ToList();
			builders.AddRange(MetaplayClient.PlayerModel.Builders.Temporary.Values.ToList());

			foreach (BuilderModel builder in builders) {
				CreateBuilderItem(builder);
			}
		}

		protected override async UniTask Idle(CancellationToken ct) {
			await UniTask.WhenAny(
				CloseButton.OnClickAsync(ct),
				OnBackgroundClickAsync(ct)
			);
		}

		protected override void HandleAndroidBackButtonPressed() {
			CloseButton.onClick.Invoke();
		}

		private void Clear() {
			foreach (Transform item in Container) {
				Destroy(item.gameObject);
			}
		}

		private void CreateBuilderItem(BuilderModel builder) {
			BuilderItem item = container.InstantiatePrefabForComponent<BuilderItem>(PrefabBuilderItem, Container);
			item.Setup(builder, CloseButton.onClick.Invoke);
		}
	}
}
