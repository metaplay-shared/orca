using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Code.UI.Map {
	[ExecuteInEditMode]
	public class CloudTile : MonoBehaviour {
		[SerializeField] private Vector2Int RenderTextureSize = new(512, 512);
		[SerializeField] private Camera Camera;
		[SerializeField] private MeshRenderer MeshRenderer;
		private static readonly int HOLES_TEXTURE = Shader.PropertyToID("_HoleTex");

		public int GridX { private set; get; }
		public int GridY { private set; get; }
		public Rect WorldRect {
			get {
				Transform tileTransform = transform; // cache property access
				Vector3 tileWorldPosition = tileTransform.position;
				Vector2 tilePosition = new(tileWorldPosition.x, tileWorldPosition.z);
				Vector2 tileSize = tileTransform.localScale;
				return new() {
					size = tileSize, // Set size first, as setting the center depends on the size internally
					center = tilePosition
				};
			}
		}

		public void SetGridPosition(int x, int y) {
			GridX = x;
			GridY = y;
			Transform tileTransform = transform;
			Vector3 tilePosition = tileTransform.position;
			Vector3 tileSize = tileTransform.localScale;
			tilePosition.x = x * tileSize.x;
			tilePosition.z = y * tileSize.y; // Note: x,y on the tile grid is x,z in world space
			tileTransform.position = tilePosition;
			UpdateCutOutTexture();
		}

		public void UpdateCutOutTexture() {
			Camera.Render();
		}

		protected void Awake() {
			ValidateTileSetup();
			SetupRenderTexture();
			UpdateCutOutTexture();
		}

		protected void OnValidate() {
			ValidateTileSetup();
			SetupRenderTexture();
		}

		private void ValidateTileSetup() {
			// check some pre-conditions and ensure the camera matches the tile as this is easy to miss
			Vector3 tileSize = transform.localScale;
			Debug.Assert(
				Math.Abs(tileSize.x - tileSize.y) < 0.001f,
				$"{nameof(CloudTile)}: Cloud tiles have to be setup as square. Make sure X and Y scale match."
			);
			Camera.orthographicSize = tileSize.x / 2.0f;
		}

		private void SetupRenderTexture() {
			RenderTexture renderTexture = Camera.targetTexture = new(RenderTextureSize.x, RenderTextureSize.y, 32);
			MaterialPropertyBlock block = new();
			MeshRenderer.GetPropertyBlock(block);
			block.SetTexture(HOLES_TEXTURE, renderTexture);
			MeshRenderer.SetPropertyBlock(block);
		}
	}
}
