using Metaplay.Core.Player;

namespace Game.Server;

public class PlayerNameValidator : PlayerRequirementsValidator {
	public override int MaxPlayerNameLength => 40;
}
