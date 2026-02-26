using System;
using UnityEngine;
using Zenject;

namespace Code.UI.Application {
	public interface IFrameRateDebugger {
		void ToggleActive();
	}

	public class FrameRateDebugger : MonoBehaviour, IInitializable, IFrameRateDebugger {
		[Inject] private IFrameRateController frameRateController;

		private const string IS_ACTIVE_KEY = "FrameRateDebugger.IsActive";

		private bool IsActive {
			get => Convert.ToBoolean(PlayerPrefs.GetInt(IS_ACTIVE_KEY, Convert.ToInt32(false)));
			set {
				enabled = value;
				PlayerPrefs.SetInt(IS_ACTIVE_KEY, Convert.ToInt32(value));
			}
		}

		private void OnGUI() {
			GUI.matrix = Matrix4x4.TRS(
				Vector3.zero,
				Quaternion.identity,
				new Vector3 (Screen.width / 1080f, Screen.height / 1920f, 1f)
			);

			GUIStyle labelStyle = new() {
				fontSize = 48,
				active = new GUIStyleState {
					textColor = Color.white
				}
			};

			GUILayout.Label($"Target: {UnityEngine.Application.targetFrameRate}", labelStyle);

			foreach (IFrameRateHandle frameRateHandle in frameRateController.GetHandles()) {
				GUILayout.Label(frameRateHandle.Context, labelStyle);
			}
		}

		public void Initialize() {
			enabled = IsActive;
		}

		public void ToggleActive() {
			IsActive = !IsActive;
		}
	}
}
