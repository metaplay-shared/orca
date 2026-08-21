using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Code.UI.Utils {
	public static class ImageUtils {
		public static async UniTask SetSpriteFromAddressableAssetsAsync(this Image target, object key, CancellationToken ct) {
			target.enabled = false;

			AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(key);
			ct.Register(() => Addressables.Release(handle));
			try {
				Sprite sprite = await handle;
				ct.ThrowIfCancellationRequested();
				target.sprite = sprite;

				target.enabled = true;
			} catch {
				Addressables.Release(handle);
				throw;
			}
		}
	}
}
