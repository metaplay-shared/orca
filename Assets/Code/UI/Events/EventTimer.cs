using Code.UI.Utils;
using Metaplay.Core.Activables;
using Metaplay.Core.Player;
using Metaplay.Unity.DefaultIntegration;
using TMPro;
using UnityEngine;

namespace Code.UI.Events {
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class EventTimer : UpdateLabel {
		protected override string SourceText => GetTime();

		protected override float UpdateIntervalSeconds => 1;

		private MetaActivableParams activableParams;

		public void Setup(MetaActivableParams activableParams) {
			this.activableParams = activableParams;
		}

		private string GetTime() {
			if (activableParams == null) {
				return "";
			}

			var currentOrNextEnabledOccasion = activableParams.Schedule
				.QueryOccasions(MetaplayClient.PlayerModel.GetCurrentLocalTime()).CurrentOrNextEnabledOccasion;

			if (currentOrNextEnabledOccasion == null) {
				return "";
			}

			var startTime = currentOrNextEnabledOccasion.Value
				.EnabledRange.Start;

			if ((startTime - MetaplayClient.PlayerModel.CurrentTime).Milliseconds < 0) {
				startTime = currentOrNextEnabledOccasion.Value
					.EnabledRange.End;
			}

			return (startTime - MetaplayClient.PlayerModel.CurrentTime).ToSimplifiedString();
		}
	}
}
