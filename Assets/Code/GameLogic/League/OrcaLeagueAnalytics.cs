// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core.Analytics;
using Metaplay.Core.League;
using Metaplay.Core.Model;
using System.Collections.Generic;

namespace Game.Logic
{
    // GAME-SPECIFIC LEAGUE EVENTS

    public static class LeagueEventCodes
    {
        public const int DivisionConcluded = 200;
    }

    [MetaSerializable]
    [FirebaseAnalyticsIgnore] // No need for firebase, this is server-only
    [BigQueryAnalyticsName("division_conclude")] // Example rename for BigQuery
    [AnalyticsEvent(LeagueEventCodes.DivisionConcluded)]
    public class OrcaDivisionEndAnalyticsEvent : DivisionEventBase
    {
        [MetaMember(1)]
        [BigQueryAnalyticsName("example_field")]
        public string ExampleField;

        [MetaMember(2)]
        [BigQueryAnalyticsName("winners")]
        public List<string> Top5PlayerNames;

        public override string EventDescription => "Division conclusion results";

        OrcaDivisionEndAnalyticsEvent() { }
        public OrcaDivisionEndAnalyticsEvent(string exampleField, List<string> top5PlayerNames)
        {
            ExampleField = exampleField;
            Top5PlayerNames = top5PlayerNames;
        }
    }
}
