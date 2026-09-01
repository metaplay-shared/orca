using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerClaimLogbookChapterReward)]
	public class PlayerClaimLogbookChapterReward : PlayerAction {
		public LogbookChapterId ChapterId { get; private set; }

		public PlayerClaimLogbookChapterReward() { }

		public PlayerClaimLogbookChapterReward(LogbookChapterId chapterId) {
			ChapterId = chapterId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Logbook.Chapters.ContainsKey(ChapterId)) {
				return ActionResult.InvalidParam;
			}

			if (player.Logbook.Chapters[ChapterId].State != ChapterState.Complete) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				player.Logbook.ClaimChapterReward(
					player.GameConfig,
					ChapterId,
					player.CurrentTime,
					player,
					player.ClientListener
				);
				LogbookChapterInfo chapterInfo = player.Logbook.Chapters[ChapterId].Info;
				RewardModel reward = new RewardModel(
					chapterInfo.RewardResources,
					chapterInfo.RewardItems,
					ChainTypeId.LevelUpRewards,
					1,
					new RewardMetadata { Type = RewardType.LogbookChapter, Chapter = ChapterId }
				);
				player.AddReward(reward);
				player.EventStream.Event(new PlayerLogbookChapterRewardClaimed(ChapterId));
			}

			return ActionResult.Success;
		}
	}
}
