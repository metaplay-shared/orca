using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Merge.AddOns.MergeItem {
	public class ProgressIndicator : MonoBehaviour {
		[SerializeField] private Sprite BlockSprite;
		private List<Image> blocks = new();

		public void SetProgress(int done, int total) {
			if (total != blocks.Count) {
				foreach (Transform child in transform) {
					Destroy(child);
				}
				blocks.Clear();

				for (int i = 0; i < total; i++) {
					GameObject block = new();
					Image blockImage = block.AddComponent<Image>();
					blockImage.sprite = BlockSprite;
					blocks.Add(blockImage);
					block.transform.SetParent(transform, false);
				}
			}

			for (int i = 0; i < blocks.Count; i++) {
				Image block = blocks[i];
				if (blocks.Count - i > done) {
					block.color = Color.white;
				} else {
					block.color = Color.gray;
				}
			}
		}
	}
}
