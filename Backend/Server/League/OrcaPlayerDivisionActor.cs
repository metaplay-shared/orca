// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Game.Logic;
using Metaplay.Cloud.Entity;
using Metaplay.Cloud.RuntimeOptions;
using Metaplay.Core;
using Metaplay.Core.League;
using Metaplay.Core.League.Player;
using Metaplay.Core.Model;
using Metaplay.Core.Rewards;
using Metaplay.Server.League;
using Metaplay.Server.League.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Game.Server.League {
	public class PersistedDivision : PersistedDivisionBase { }

	[EntityConfig]
	public class DivisionConfig : DivisionEntityConfigBase {
		public override Type EntityActorType => typeof(OrcaPlayerDivisionActor);
	}

	[MetaSerializableDerived(1)]
	public class OrcaDivisionServerModel : DivisionServerModelBase<OrcaPlayerDivisionModel> {
		[MetaMember(1)] public List<int> PlaceholderParticipants = new List<int>();

		private int desiredPlayerCount;

		/// <inheritdoc />
		public override void OnModelServerTick(
			OrcaPlayerDivisionModel readOnlyModel,
			IServerActionDispatcher actionDispatcher
		) {
			// Delete placeholder participants if we have too many
			if (readOnlyModel.Participants.Count > desiredPlayerCount && PlaceholderParticipants.Count > 0) {
				readOnlyModel.Log.Debug(
					"Removing placeholder participant: {ParticipantCount} participants, {PlaceholderCount} placeholders, {DesiredCount} desired",
					readOnlyModel.Participants.Count,
					PlaceholderParticipants.Count,
					desiredPlayerCount
				);

				int participantToRemove = PlaceholderParticipants[0];
				PlaceholderParticipants.RemoveAt(0);
				actionDispatcher.ExecuteAction(new DivisionParticipantRemove(participantToRemove));
			}
		}

		/// <inheritdoc />
		public override void OnFastForwardModel(OrcaPlayerDivisionModel model, MetaDuration elapsedTime) {
			desiredPlayerCount = RuntimeOptionsRegistry.Instance.GetCurrent<LeagueManagerOptions>()
				.DivisionDesiredParticipantCount;

			model.Log.Debug(
				"Server OnFastForwardModel: {ParticipantCount} participants, {PlaceholderCount} placeholders, {DesiredCount} desired",
				model.Participants.Count,
				PlaceholderParticipants.Count,
				desiredPlayerCount
			);
			// Add placeholder participants if we have too few
			if (model.Participants.Count < desiredPlayerCount) {
				int numToAdd = desiredPlayerCount - model.Participants.Count;
				for (int i = 0; i < numToAdd; i++) {
					int newParticipantIdx = model.NextParticipantIdx++;
					model.Participants.Add(newParticipantIdx, new OrcaPlayerDivisionParticipantState());
					((IPlayerDivisionParticipantState)model.Participants[newParticipantIdx]).InitializeForPlayer(
						newParticipantIdx,
						new PlayerDivisionAvatarBase.Default(
							FormattableString.Invariant($"Placeholder Player {newParticipantIdx}")
						)
					);
					PlaceholderParticipants.Add(newParticipantIdx);
				}

				(model as IDivisionModel).RefreshScores();
			}
		}
	}

	public sealed class OrcaPlayerDivisionActor : PlayerDivisionActorBase<OrcaPlayerDivisionModel,
		OrcaDivisionServerModel, PersistedDivision, LeagueManagerOptions> {
		public OrcaPlayerDivisionActor(EntityId entityId) : base(entityId) { }

		protected override IDivisionRewards CalculateDivisionRewardsForParticipant(int participantIdx) {
			if (!Model.Participants.TryGetValue(participantIdx, out OrcaPlayerDivisionParticipantState state))
				return null;

			// No rewards for players who did nothing.
			if (state.DivisionScore.MergeScore == 0)
				return null;

			// Could get these from GameConfig
			int baseGemsReward = 10;
			int actualGemsReward;

			if (state.SortOrderIndex == 0) // First player gets triple reward
				actualGemsReward = baseGemsReward * 3;
			else if (state.SortOrderIndex < 3) // 2nd and 3rd get double reward
				actualGemsReward = baseGemsReward * 2;
			else
				actualGemsReward = baseGemsReward;

			return new DivisionPlayerRewardsBase.Default(
				new List<MetaPlayerRewardBase> {
					new RewardCurrency(CurrencyTypeId.Gems, actualGemsReward),
				}
			);
		}

		protected override IDivisionHistoryEntry GetDivisionHistoryEntryForPlayer(
			int participantIdx,
			IDivisionRewards resolvedRewards
		) {
			if (!Model.Participants.TryGetValue(participantIdx, out OrcaPlayerDivisionParticipantState state))
				return null;

			return new OrcaPlayerDivisionHistoryEntry(
				_entityId,
				Model.DivisionIndex,
				resolvedRewards,
				state.DivisionScore,
				state.SortOrderIndex
			);
		}

		/// <inheritdoc />
		protected override IDivisionParticipantConclusionResult GetParticipantResult(int participantIdx) {
			if (!Model.Participants.TryGetValue(participantIdx, out OrcaPlayerDivisionParticipantState state))
				return null;

			return new OrcaPlayerDivisionConclusionResult(
				state.ParticipantId,
				state.PlayerAvatar,
				state.SortOrderIndex,
				Model.Participants.Count,
				state.DivisionScore
			);
		}

		protected override async Task ConcludeSeason() {
			await base.ConcludeSeason();

			// Emit custom example event when division is concluded

			List<string> top5PlayerNames =
				Model.Participants
					.OrderBy(keySelector: kv => kv.Value.SortOrderIndex)
					.Take(5)
					.Select(kv => kv.Value.PlayerAvatar.DisplayName)
					.ToList();

			Model.EventStream.Event(
				new OrcaDivisionEndAnalyticsEvent(
					exampleField: "my_example_value",
					top5PlayerNames: top5PlayerNames
				)
			);
		}
	}
}
