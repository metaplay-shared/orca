using System;
using System.Runtime.CompilerServices;
using Metaplay.Unity;
using TMPro;
using UnityEngine;

namespace Code.UI.Application {
	public class LoadingScreenMetadataPresenter : MonoBehaviour {
		[SerializeField] private TMP_Text Text;

		private async void OnEnable() {
			GuestCredentials credentials = await CredentialsStore.TryGetGuestCredentialsAsync("");
			if (credentials != null) {
				Text.text = credentials.PlayerId.ToString();
			}
		}
	}
}
