using JetBrains.Annotations;
using Metaplay.Core.Analytics;
using Metaplay.Core.Model;
using Metaplay.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.UI.Application {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class MetaplayAnalyticsController : IMetaplayClientAnalyticsDelegate {
		private readonly IReadOnlyList<IAnalyticsService> analyticsServices;

		public MetaplayAnalyticsController(
			List<IAnalyticsService> analyticsServices
		) {
			this.analyticsServices = analyticsServices;
		}

		public void OnAnalyticsEvent(AnalyticsEventSpec eventSpec, AnalyticsEventBase payload, IModel model) {
			foreach (IAnalyticsService analyticsService in analyticsServices) {
				try {
					analyticsService.OnAnalyticsEvent(eventSpec, payload, model);
				} catch (Exception ex) {
					Debug.LogException(ex);
				}
			}
		}
	}
}
