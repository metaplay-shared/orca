using System;
using System.Runtime.CompilerServices;
using Metaplay.Unity;
using TMPro;
using UnityEngine;

namespace Code.UI.Application {
	public class LoadingScreenMetadataPresenter : MonoBehaviour {
		[SerializeField] private TMP_Text Text;

		private async void OnEnable() {
			if (MetaplayClient.PlayerModel != null) {
				Text.text = MetaplayClient.PlayerModel.PlayerId.ToString();
			}
		}
	}
}
