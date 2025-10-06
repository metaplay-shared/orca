using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Orca.Common;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Object = UnityEngine.Object;

namespace Code.UI.Core {
	public interface IUIRootProviderHandle<out TUIRoot, TUIHandle> : IDisposable
		where TUIRoot : class, IUIRoot<TUIHandle>
		where TUIHandle : class, IUIHandle {
		TUIRoot LoadedUIRoot { get; }
	}

	public interface IUIRootProvider {
		UniTask PreloadUIRoot<TUIRoot, TUIHandle>(CancellationToken ct)
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle;

		UniTask<Option<IUIRootProviderHandle<TUIRoot, TUIHandle>>> LoadUIRoot<TUIRoot, TUIHandle>(
			TUIHandle handle,
			Option<string> prefabName,
			CancellationToken ct
		)
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle;

		bool UIRootExists(string prefabName);
	}

	[UsedImplicitly]
	public class UIRootProvider : IUIRootProvider, IInitializable, IDisposable {
		//private const string EMPTY_FILE_ADDRESS = "UIRoots/_empty.txt";

		//private Option<AsyncOperationHandle<Object>> emptyFileHandle;

		[Inject] private DiContainer container;

		private readonly List<AsyncOperationHandle<GameObject>> preloadedUIRootHandles =
			new List<AsyncOperationHandle<GameObject>>();

		public UniTask PreloadUIRoot<TUIRoot, TUIHandle>(CancellationToken ct)
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle {
			string assetKey = GetAssetKey<TUIRoot, TUIHandle>(prefabName: default);
			AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(assetKey);
			preloadedUIRootHandles.Add(handle);
			return handle.ToUniTask(cancellationToken: ct);
		}

		public async UniTask<Option<IUIRootProviderHandle<TUIRoot, TUIHandle>>> LoadUIRoot<TUIRoot, TUIHandle>(
			TUIHandle uiHandle,
			Option<string> prefabName,
			CancellationToken ct
		)
			where TUIRoot : MonoBehaviour, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle {
			var assetKey = GetAssetKey<TUIRoot, TUIHandle>(prefabName);
			var assetLoadHandle = Addressables.LoadAssetAsync<GameObject>(assetKey);

			try {
				GameObject template = await assetLoadHandle;

				if (template == null) {
					Debug.LogError($"Unable to load UI root from {assetKey}.");
					return default;
				}

				GameObject topLayer = GameObject.Find("TopLayer");
				var uiContainer = container.CreateSubContainer();
				uiContainer.BindInterfacesAndSelfTo<TUIHandle>().FromInstance(uiHandle).AsSingle();
				var templateOriginalActivity = template.activeSelf;
				template.gameObject.SetActive(false);
				var uiRoot = uiContainer.InstantiatePrefabForComponent<TUIRoot>(template, topLayer.transform);
				var rt = uiRoot.GetComponent<RectTransform>();
				rt.anchorMin = Vector2.zero;
				rt.anchorMax = Vector2.one;
				rt.sizeDelta = Vector2.zero;
				uiRoot.gameObject.SetActive(true);
				template.SetActive(templateOriginalActivity);
				var handle = new Handle<TUIRoot, TUIHandle>(uiRoot.gameObject, uiRoot, assetLoadHandle);
				ct.ThrowIfCancellationRequested();
				return handle;
			} catch {
				Addressables.Release(assetLoadHandle);
				throw;
			}
		}

		public bool UIRootExists(string prefabName) {
			var assetKey = $"Popup/{prefabName}.prefab";
			bool addressableResourceExists = Addressables.ResourceLocators.Any(
				resourceLocator => resourceLocator.Keys.Contains(assetKey)
			);
			return addressableResourceExists;
		}

		private static string GetAssetKey<TUIRoot, TUIHandle>(Option<string> prefabName)
			where TUIRoot : class, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle {
			return $"Popup/{prefabName.GetOrElse(typeof(TUIRoot).Name)}.prefab";
		}

		private class Handle<TUIRoot, TUIHandle> : IUIRootProviderHandle<TUIRoot, TUIHandle>
			where TUIRoot : class, IUIRoot<TUIHandle>
			where TUIHandle : class, IUIHandle {
			private readonly GameObject gameObject;
			private readonly AsyncOperationHandle<GameObject> assetLoadHandle;
			public TUIRoot LoadedUIRoot { get; }

			public Handle(
				GameObject gameObject,
				TUIRoot loadedUIRoot,
				AsyncOperationHandle<GameObject> assetLoadHandle
			) {
				this.gameObject = gameObject;
				this.assetLoadHandle = assetLoadHandle;
				LoadedUIRoot = loadedUIRoot;
			}

			public void Dispose() {
				gameObject.SetActive(false);
				Object.Destroy(gameObject);
				Addressables.Release(assetLoadHandle);
				// This is a slow operation but will help keep the memory tidy.
				// As this happens usually after a UI is closed it should not be too noticeable to the player.
				Resources.UnloadUnusedAssets();
			}
		}

		public void Initialize() {
			//emptyFileHandle = Addressables.LoadAssetAsync<Object>(EMPTY_FILE_ADDRESS);
		}

		public void Dispose() {
			/*
			foreach (var handle in emptyFileHandle) {
				Addressables.Release(handle);
			}

			emptyFileHandle = default;
			*/

			foreach (var handle in preloadedUIRootHandles) {
				Addressables.Release(handle);
			}

			preloadedUIRootHandles.Clear();
		}
	}
}
