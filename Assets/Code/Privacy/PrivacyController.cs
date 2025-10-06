using Code.UI.Privacy;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using System;
using System.Threading;
using UnityEngine;

namespace Code.Privacy {
	public interface IPrivacyController {
		bool IsTermsOfServiceAccepted { get; }
		UniTask AcceptTermsOfServiceAsync(CancellationToken ct);
	}

	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class PrivacyController : IPrivacyController {
		private const string TERMS_OF_SERVICE_ACCEPTED_KEY = "TOS_ACCEPTED";
		public bool IsTermsOfServiceAccepted {
			get => Convert.ToBoolean(PlayerPrefs.GetInt(TERMS_OF_SERVICE_ACCEPTED_KEY, Convert.ToInt32(false)));
			private set {
				PlayerPrefs.SetInt(TERMS_OF_SERVICE_ACCEPTED_KEY, Convert.ToInt32(value));
				PlayerPrefs.Save();
			}
		}

		public UniTask AcceptTermsOfServiceAsync(CancellationToken ct) {
			if (IsTermsOfServiceAccepted) {
				return UniTask.CompletedTask;
			}

			//return PrivacyPopup.ShowPrivacyPopup(ct).ContinueWith(AcceptTermsOfService);
			return UniTask.CompletedTask;
		}

		private void AcceptTermsOfService() {
			IsTermsOfServiceAccepted = true;
		}
	}
}
