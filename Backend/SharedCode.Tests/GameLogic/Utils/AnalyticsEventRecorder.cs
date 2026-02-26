using System;
using System.Collections.Generic;
using Metaplay.Core.Analytics;

namespace CloudCore.Tests.GameLogic.Utils {
	public class AnalyticsEventRecorder<TContext, TEvent> where TEvent : AnalyticsEventBase {
		public bool IsDebugOn = false;
		public List<TEvent> Events { get; private set; } = new List<TEvent>();

		public int TotalCount => Events.Count;

		public void RecordEvent(TContext context, TEvent payload) {
			if (IsDebugOn) {
				Console.Out.WriteLine($"Recording event: {payload}");
			}

			Events.Add(payload);
		}

		public int EventCount(Type type) {
			int count = 0;
			foreach (var payload in Events) {
				if (payload.GetType() == type) {
					count++;
				}
			}

			return count;
		}

		public TEvent First(Type type) {
			foreach (var payload in Events) {
				if (payload.GetType() == type) {
					return payload;
				}
			}

			return null;
		}

		public void Clear() {
			Events.Clear();
		}

		public override string ToString() {
			string result = $"Analytics events ({TotalCount} in total)";
			for (int i = 0; i < Events.Count; i++) {
				result += $"\n {i}: {Events[i]}";
			}

			return result;
		}
	}
}
