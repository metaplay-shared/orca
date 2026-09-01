using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using Cysharp.Threading.Tasks;
using Game.Logic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.U2D;

namespace Code.UI.AssetManagement {
	public class AddressableManager : IDisposable {
		private readonly HashSet<object> keys = new();
		private readonly List<AsyncOperationHandle> handles = new();

		public bool IsDisposed { get; private set; }

		public void Dispose() {
			if (IsDisposed) {
				return;
			}

			foreach (var handle in handles) {
				Addressables.Release(handle);
			}

			IsDisposed = true;
		}

		public async UniTask PreloadSetAsync(object key, Type type) {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}

			var results = await Addressables.LoadResourceLocationsAsync(key, type).Task;
			var loadKeys = results.Select(r => r.PrimaryKey).ToArray();

			var task = (UniTask)typeof(AddressableManager)
				.GetMethod("PreloadAsync")
				?.MakeGenericMethod(type)
				.Invoke(this, new object[] { loadKeys })!;

			await task;

			//await PreloadAsync(loadKeys);
		}

		public async UniTask PreloadSetAsync<T>(object key, IProgress<float> progress = null) {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}

			AsyncOperationHandle<IList<IResourceLocation>> handle = Addressables.LoadResourceLocationsAsync(key, typeof(T));
			using IDisposable _ = Observable.EveryUpdate().Select(_ => handle.PercentComplete)
				.Subscribe(value => progress?.Report(value / 2f));
			IList<IResourceLocation> results = await handle;
			progress?.Report(0.5f);
			object[] loadKeys = results.Select(r => (object)r.PrimaryKey).ToArray();

			Progress<float> setProgress = new();
			setProgress.ProgressChanged += (_, value) => progress?.Report(0.5f + value / 2f);
			await PreloadAsync<T>(loadKeys, setProgress);
			progress?.Report(1f);
		}

		public async UniTask PreloadAsync<T>(object loadKey, IProgress<float> progress = null) {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}

			if (loadKey is null) {
				throw new ArgumentNullException(nameof(loadKey));
			}

			AsyncOperationHandle<T> loadHandle = Addressables.LoadAssetAsync<T>(loadKey);
			handles.Add(loadHandle);

			using IDisposable _ = Observable.EveryUpdate().Select(_ => loadHandle.PercentComplete)
				.Subscribe(value => progress?.Report(value));
			await loadHandle;
			progress?.Report(1f);

			if (loadHandle.Status == AsyncOperationStatus.Failed) {
				ExceptionDispatchInfo.Capture(loadHandle.OperationException).Throw();
			}

			keys.Add(loadKey);
		}

		public async UniTask PreloadAsync<T>(object[] loadKeys, IProgress<float> progress = null) {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}

			if (loadKeys is null) {
				throw new ArgumentNullException(nameof(loadKeys));
			}

			if (loadKeys.Length == 0) {
				throw new ArgumentException(nameof(keys));
			}

			AsyncOperationHandle[] loadHandles = loadKeys.Select(Addressables.LoadAssetAsync<T>)
				.Select(h => (AsyncOperationHandle)h).ToArray();
			handles.AddRange(loadHandles);

			using IDisposable _ = Observable.EveryUpdate()
				.Select(_ => loadHandles.Select(handle => handle.PercentComplete).Sum() / loadHandles.Length)
				.Subscribe(value => progress?.Report(value));

			var tasks = loadHandles.Select(h => h.Task.AsUniTask());
			await UniTask.WhenAll(tasks);
			progress?.Report(1f);

			foreach (var handle in loadHandles) {
				if (handle.Status == AsyncOperationStatus.Failed) {
					ExceptionDispatchInfo.Capture(handle.OperationException).Throw();
				}
			}

			foreach (var key in loadKeys) {
				keys.Add(key);
			}
		}

		public T Get<T>(object key) {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}

			if (key is null) {
				throw new ArgumentNullException(nameof(key));
			}

			if (!keys.Contains(key)) {
				throw new InvalidOperationException($"{key} is not loaded. Call {nameof(PreloadAsync)} first.");
			}

			var handle = Addressables.LoadAssetAsync<T>(key);
			if (!handle.IsDone) {
				Addressables.Release(handle);

				throw new InvalidOperationException($"{key} is not loaded. Call {nameof(PreloadAsync)} first.");
			}

			if (handle.Status == AsyncOperationStatus.Failed) {
				Addressables.Release(key);

				ExceptionDispatchInfo.Capture(handle.OperationException).Throw();
			}

			var result = handle.Result;
			Addressables.Release(handle);

			return (T)result;
		}

		public async UniTask<T> GetLazy<T>(object key) {
			if (IsDisposed) {
				throw new ObjectDisposedException(GetType().Name);
			}

			if (key is null) {
				throw new ArgumentNullException(nameof(key));
			}

			if (!keys.Contains(key)) {
				await PreloadAsync<T>(key);
			}

			var handle = Addressables.LoadAssetAsync<T>(key);
			if (!handle.IsDone) {
				Addressables.Release(handle);

				throw new InvalidOperationException($"{key} is not loaded. Call {nameof(PreloadAsync)} first.");
			}

			if (handle.Status == AsyncOperationStatus.Failed) {
				Addressables.Release(key);

				ExceptionDispatchInfo.Capture(handle.OperationException).Throw();
			}

			var result = handle.Result;
			Addressables.Release(handle);

			return (T)result;
		}
	}

	public static class AddressableManagerExtensions {
		public static Sprite GetItemIcon(this AddressableManager addressableManager, ChainInfo chainInfo) {
			return addressableManager.GetItemIcon(chainInfo.Type, chainInfo.Level);
		}

		public static Sprite GetItemIcon(this AddressableManager addressableManager, ChainTypeId chainId, int level) {
			return addressableManager.GetItemIcon(chainId.Value, level);
		}

		public static Sprite GetItemIcon(this AddressableManager addressableManager, LevelId<ChainTypeId> itemInfo) {
			return addressableManager.GetItemIcon(itemInfo.Type, itemInfo.Level);
		}

		public static Sprite GetItemIcon(this AddressableManager addressableManager, string chainId, int level) {
			SpriteAtlas spriteAtlas = addressableManager.Get<SpriteAtlas>($"Chains/{chainId}");
			var spriteName = $"{chainId}{level}";
			Sprite sprite = spriteAtlas.GetSprite(spriteName);
			Debug.Assert(sprite != null, $"Couldn't find item sprite: '{spriteName}'");
			return sprite;
		}
	}

	public static class AddressableUtils {
		public static string GetItemIconAddress(LevelId<ChainTypeId> itemInfo) {
			return GetItemIconAddress(itemInfo.Type.Value, itemInfo.Level);
		}

		public static string GetItemIconAddress(ChainTypeId chainType, int level) {
			return GetItemIconAddress(chainType.Value, level);
		}

		public static string GetItemIconAddress(string chainId, int level) {
			return $"Chains/{chainId}[{chainId}{level}]";
		}

		public static string GetItemCategoryIconAddress(CategoryId category) {
			return $"ItemCategoriesIcons[{category.Value}]";
		}
	}
}
