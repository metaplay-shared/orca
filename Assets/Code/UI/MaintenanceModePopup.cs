using Code.UI.Utils;
using Cysharp.Threading.Tasks;
using Metaplay.Unity;
using Metaplay.Unity.ConnectionStates;
using System.Globalization;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Code.UI {
	public class MaintenanceModePopup : MonoBehaviour {
		[SerializeField] private Button ReconnectButton;
		[SerializeField] private TMP_Text EstimatedEndTime;

		public static async UniTask ShowMaintenanceModePopup(
			ConnectionLostEvent connectionLost,
			TerminalError.InMaintenance inMaintenance
		) {
			AsyncOperationHandle<GameObject>
				handle = Addressables.InstantiateAsync("Popup/MaintenanceModePopup.prefab");
			GameObject go = await handle;
			MaintenanceModePopup presenter = go.GetComponent<MaintenanceModePopup>();
			await presenter.Show(connectionLost, inMaintenance, go.GetCancellationTokenOnDestroy());
			Destroy(go);
		}

		private UniTask Show(
			ConnectionLostEvent connectionLost,
			TerminalError.InMaintenance inMaintenance,
			CancellationToken ct
		) {
			EstimatedEndTime.gameObject.SetActive(MetaplaySDK.MaintenanceMode.EstimatedMaintenanceOverAt.HasValue);
			string datePattern = string.Join(
				' ',
				CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern,
				CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern
			);
			string endTime =
				MetaplaySDK.MaintenanceMode.EstimatedMaintenanceOverAt?.ToDateTime().ToLocalTime()
					.ToString(datePattern) ??
				string.Empty;
			EstimatedEndTime.text = Localizer.Localize(
				"MaintenanceModePopup.EstimatedEndTime",
				endTime
			);
			return ReconnectButton.OnClickAsync(ct);
		}
	}
}
