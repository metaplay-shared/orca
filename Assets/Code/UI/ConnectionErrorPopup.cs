using Cysharp.Threading.Tasks;
using Metaplay.Core;
using Metaplay.Unity;
using System.Text;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Code.UI {
	public class ConnectionErrorPopup : MonoBehaviour {
		[SerializeField] private TMP_Text ConnectionErrorInfoText;
		[SerializeField] private TMP_Text AutoReconnectInfoText;
		[SerializeField] private Button ReconnectButton;
		[SerializeField] private float AutoReconnectTimeout = 5f;

		private float autoReconnectTimer;

		/// <summary>
		/// Convert a <see cref="ConnectionLostEvent"/> into a human-readable string, for displaying in the UI.
		/// This implementation shows quite a lot of technical detail which is useful for developers, but for
		/// real players, you'd want to show something more compact.
		/// </summary>
		/// <param name="connectionLost"></param>
		/// <returns>Technical description of the connection loss event, mainly intended for developers.</returns>
		static string CreateConnectionLostInfoText(ConnectionLostEvent connectionLost) {
			StringBuilder info = new StringBuilder();

			#if UNITY_EDITOR
				// EnglishLocalizedReason and TechnicalErrorCode should typically be shown to players.
				info.AppendLine($"* Reason: {connectionLost.EnglishLocalizedReason}");
				info.AppendLine($"* Technical code: {connectionLost.TechnicalErrorCode}");

				// TechnicalErrorString and ExtraTechnicalInfo are intended for analytics.
				info.AppendLine();
				info.AppendLine($"* Technical error string: {connectionLost.TechnicalErrorString}");
				if (!string.IsNullOrEmpty(connectionLost.ExtraTechnicalInfo))
					info.AppendLine($"* Additional technical info: {connectionLost.ExtraTechnicalInfo}");

				// More detailed technical info that's mainly useful for developers.
				info.AppendLine();
				info.AppendLine($"* Technical error: {PrettyPrint.Compact(connectionLost.TechnicalError)}");
			#else
				info.Append("Something went wrong. Try to reconnect");
			#endif

			return info.ToString();
		}

		private void Update() {
			AutoReconnectInfoText.text = $"Reconnecting in {autoReconnectTimer:0} seconds...";
			autoReconnectTimer -= Time.deltaTime;

			if (autoReconnectTimer > 0) {
				return;
			}

			ReconnectButton.onClick.Invoke();
		}

		private UniTask Show(ConnectionLostEvent connectionLost) {
			string infoText = CreateConnectionLostInfoText(connectionLost);
			ConnectionErrorInfoText.text = infoText;
			autoReconnectTimer = AutoReconnectTimeout;
			return ReconnectButton.OnClickAsync(gameObject.GetCancellationTokenOnDestroy());
		}

		public static async UniTask ShowConnectionErrorPopup(ConnectionLostEvent connectionLost) {
			AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync("Popup/ConnectionErrorPopup.prefab");
			GameObject go = await handle;
			ConnectionErrorPopup presenter = go.GetComponent<ConnectionErrorPopup>();
			await presenter.Show(connectionLost);
			Destroy(go);
		}
	}
}
