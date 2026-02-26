using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Code.UI.Utils {
	public class SizeAdjuster : MonoBehaviour {
		[SerializeField] private RectTransform TargetComponent;
		[SerializeField] private RectTransform SourceComponent;

		private async void Start() {
			if (TargetComponent == null) {
				TargetComponent = GetComponent<RectTransform>();
			}
			await UniTask.Yield();
			TargetComponent.sizeDelta = SourceComponent.sizeDelta;
		}
	}
}
