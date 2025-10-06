// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.League;
using Metaplay.Core.League.Player;
using Metaplay.Core.Model;
using System;
using static System.FormattableString;

namespace Game.Logic
{
    [MetaSerializableDerived(1)]
    public class OrcaPlayerDivisionScore : IDivisionScore, IDivisionContribution
    {
        [MetaMember(1)] public int      MergeScore;
        [MetaMember(2)] public MetaTime LastActionAt;

        int IDivisionScore.CompareTo(IDivisionScore untypedOther)
        {
            OrcaPlayerDivisionScore other = (OrcaPlayerDivisionScore)untypedOther;

            if (MergeScore < other.MergeScore)
                return -1;
            if (MergeScore > other.MergeScore)
                return +1;

            if (LastActionAt < other.LastActionAt)
                return +1;
            if (LastActionAt > other.LastActionAt)
                return -1;

            return 0;
        }
    }

    /// <summary>
    /// Player score event example for when player levels up.
    /// </summary>
    [MetaSerializableDerived(1)]
    public class OrcaPlayerDivisionMergeScoreEvent : DivisionScoreEventBase<OrcaPlayerDivisionScore>
    {
        [MetaMember(1)] public MetaTime         EventAt  { get; set; }
        [MetaMember(3)] public int              AddedMergeScore { get; set; }

        OrcaPlayerDivisionMergeScoreEvent() { }
        public OrcaPlayerDivisionMergeScoreEvent(MetaTime eventAt, int mergeScore)
        {
            EventAt  = eventAt;
            AddedMergeScore = mergeScore;
        }

        public override void AccumulateToContribution(OrcaPlayerDivisionScore contribution)
        {
            contribution.MergeScore += AddedMergeScore;
            contribution.LastActionAt = EventAt;
        }
    }

    /// <summary>
    /// Example per-participant state. We don't add anything here.
    /// </summary>
    [MetaSerializableDerived(1)]
    public class OrcaPlayerDivisionParticipantState : PlayerDivisionParticipantStateBase<OrcaPlayerDivisionScore, OrcaPlayerDivisionScore, PlayerDivisionAvatarBase.Default>
    {
        /// <inheritdoc />
        public override string ParticipantInfo => Invariant($"Score: {PlayerContribution.MergeScore}");
    }

    /// <summary>
    /// Example per-division state. We don't add anything here.
    /// </summary>
    [MetaSerializableDerived(3)]
    [SupportedSchemaVersions(1,2)]
    public class OrcaPlayerDivisionModel : PlayerDivisionModelBase<OrcaPlayerDivisionModel, OrcaPlayerDivisionParticipantState, OrcaPlayerDivisionScore, PlayerDivisionAvatarBase.Default>
    {
        public override int TicksPerSecond => 1;

        public override void OnTick()
        {
            // Nothing
        }

        public override void OnFastForwardTime(MetaDuration elapsedTime)
        {
            // Nothing
        }

        public override OrcaPlayerDivisionScore ComputeScore(int participantIndex)
        {
            // No special score computation logic.
            return Participants[participantIndex].PlayerContribution;
        }

        [MigrationFromVersion(1)]
        void MigrateParticipantData()
        {
            #pragma warning disable CS0618
            if (ServerModel != null)
            {
                foreach ((EntityId participantId, OrcaPlayerDivisionParticipantState participantState) in LegacyParticipants)
                {
                    // Set participant index
                    participantState.ParticipantIndex = NextParticipantIdx++;
                    participantState.ParticipantId    = participantId;
                    // Update ServerModel
                    ServerModel.ParticipantIndexToEntityId.Add(participantState.ParticipantIndex, participantId);
                    Participants.Add(participantState.ParticipantIndex, participantState);
                }
                LegacyParticipants = null;
            }
            else
                throw new InvalidOperationException("ServerModel is null. Cannot migrate participant data.");
            #pragma warning restore CS0618
        }
    }
}
