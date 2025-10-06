using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Code.UI.Tutorial {
	public abstract class Highlight : IDisposable {
		[Inject] protected Blackout blackout;

		protected readonly List<Highlighted> highlightedObjects = new();

		private Transform TopLayer => GameObject.Find("TopLayer").transform;

		public async UniTask Run(params GameObject[] highlightedObjects) {
			this.highlightedObjects.AddRange(
				highlightedObjects
					.Select(
						go => new Highlighted {
							GameObject = go,
							OriginalParent = go.transform.parent,
							ChildIndex = go.transform.GetSiblingIndex(),
							ProcessOnBlackoutClick = go.GetComponent<HighlightableElement>() != null && go.GetComponent<HighlightableElement>().ProcessOnBlackoutClick
						}
					)
					.ToList()
			);

			PreProcess();
			CaptureObjects();
			await blackout.FadeIn();

			await Wait();

			await blackout.FadeOut();
			PostProcess();
		}

		private void CaptureObjects() {
			foreach (var highlightedObject in highlightedObjects)
			{
				highlightedObject.GameObject.transform.SetParent(TopLayer, true);
			}
		}

		protected abstract void PreProcess();

		protected abstract UniTask Wait();

		protected abstract void PostProcess();

		public void Dispose() {
			foreach (Highlighted highlighted in highlightedObjects) {
				if (highlighted.GameObject == null ||
					!highlighted.GameObject.activeInHierarchy) {
					return;
				}

				highlighted.GameObject.transform.SetParent(highlighted.OriginalParent, true);
				highlighted.GameObject.transform.SetSiblingIndex(highlighted.ChildIndex);
			}
		}
	}

	public class Highlighted {
		public GameObject GameObject;
		public Transform OriginalParent;
		public int ChildIndex = 0;
		public bool ProcessOnBlackoutClick = false;
	}
}
