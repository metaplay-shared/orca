using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerOpenLogbookChapter)]
	public class PlayerOpenLogbookChapter : PlayerAction {
		public LogbookChapterId ChapterId { get; private set; }

		public PlayerOpenLogbookChapter() { }

		public PlayerOpenLogbookChapter(LogbookChapterId chapterId) {
			ChapterId = chapterId;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (!player.Logbook.Chapters.ContainsKey(ChapterId)) {
				return ActionResult.InvalidParam;
			}

			if (player.Logbook.Chapters[ChapterId].State != ChapterState.Opening) {
				return ActionResult.InvalidState;
			}

			if (commit) {
				player.Logbook.OpenChapter(ChapterId);
				player.ClientListener.OnLogbookChapterModified(ChapterId);
			}

			return ActionResult.Success;
		}
	}
}
