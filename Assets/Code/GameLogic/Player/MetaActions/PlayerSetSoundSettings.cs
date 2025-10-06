using Metaplay.Core.Model;

namespace Game.Logic {
	[ModelAction(ActionCodes.PlayerSetSoundSettings)]
	public class PlayerSetSoundSettings : PlayerAction {
		public bool Sound { get; private set; }
		public bool Music { get; private set; }

		public PlayerSetSoundSettings() { }

		public PlayerSetSoundSettings(bool sound, bool music) {
			Sound = sound;
			Music = music;
		}

		public override MetaActionResult Execute(PlayerModel player, bool commit) {
			if (commit) {
				player.PrivateProfile.SoundSettings.SoundEnabled = Sound;
				player.PrivateProfile.SoundSettings.MusicEnabled = Music;
			}

			return ActionResult.Success;
		}
	}
}
