using Cysharp.Threading.Tasks;
using Metaplay.Unity;
using Metaplay.Unity.ConnectionStates;
using System.Threading;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Code.UI {
	public class ForcedUpdatePopup : MonoBehaviour {
		[SerializeField] private Button UpdateButton;

		private void Awake() {
			UpdateButton.OnClickAsObservable().Subscribe(_ => NavigateToStore()).AddTo(gameObject);
		}

		private void NavigateToStore() {
			string url = GetStoreURL();
			UnityEngine.Application.OpenURL(url);
		}

		private static string GetStoreURL() {
			return UnityEngine.Application.platform switch {
				RuntimePlatform.Android      => "https://play.google.com/",
				RuntimePlatform.IPhonePlayer => "https://apps.apple.com/",
				_                            => ""
			};
		}

		public static async UniTask ShowForcedUpdatePopup(
			ConnectionLostEvent connectionLost,
			TerminalError.LogicVersionMismatch logicVersionMismatch
		) {
			AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync("Popup/ForcedUpdatePopup.prefab");
			GameObject go = await handle;
			ForcedUpdatePopup presenter = go.GetComponent<ForcedUpdatePopup>();
			await presenter.Show(connectionLost, logicVersionMismatch, go.GetCancellationTokenOnDestroy());
			Destroy(go);
		}

		private UniTask Show(
			ConnectionLostEvent connectionLost,
			TerminalError.LogicVersionMismatch logicVersionMismatch,
			CancellationToken ct
		) {
			return UniTask.Never(ct);
		}
	}
}
