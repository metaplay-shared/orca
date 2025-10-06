using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSetLatestInfoUrl)]
	public class PlayerSetLatestInfoUrl : PlayerAction
	{
		public string Url { get; private set; }

		public PlayerSetLatestInfoUrl() { }

		public PlayerSetLatestInfoUrl(string url) {
			Url = url;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (commit) {
				player.LatestInfoUrl = Url;
			}

			return MetaActionResult.Success;
		}
	}
}
