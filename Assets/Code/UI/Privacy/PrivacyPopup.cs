using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Code.UI.Privacy {
	public class PrivacyPopup : MonoBehaviour {
		[SerializeField] private Button AcceptButton;

		public static async UniTask ShowPrivacyPopup(CancellationToken ct) {
			AsyncOperationHandle<GameObject>
				handle = Addressables.InstantiateAsync("Popup/PrivacyPopup.prefab");
			GameObject go = await handle;
			DontDestroyOnLoad(go);
			PrivacyPopup presenter = go.GetComponent<PrivacyPopup>();
			CancellationTokenSource cts =
				CancellationTokenSource.CreateLinkedTokenSource(ct, go.GetCancellationTokenOnDestroy());
			await presenter.Show(cts.Token);
			Destroy(go);
		}

		private UniTask Show(
			CancellationToken ct
		) {
			return AcceptButton.OnClickAsync(ct);
		}
	}
}
