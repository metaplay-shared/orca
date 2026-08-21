// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic
{
    /// <summary>
    /// Game-specific player action class, which attaches all game-specific actions to <see cref="PlayerModel"/>.
    /// </summary>
    public abstract class PlayerAction : PlayerActionCore<PlayerModel>
    {
    }

    /// <summary>
    /// Game-specific <see cref="PlayerUnsynchronizedServerActionCore{TModel}"/>
    /// </summary>
    public abstract class PlayerUnsynchronizedServerAction : PlayerUnsynchronizedServerActionCore<PlayerModel>
    {
    }

    /// <summary>
    /// Game-specific <see cref="PlayerSynchronizedServerActionCore{TModel}"/>
    /// </summary>
    public abstract class PlayerSynchronizedServerAction : PlayerSynchronizedServerActionCore<PlayerModel>
    {
    }

    public abstract class PlayerTransactionFinalizingAction : PlayerTransactionFinalizingActionCore<PlayerModel>
    {
    }
    
    /// <summary>
    /// Admin action to give a player gold and gems
    /// </summary>
    [ModelAction(ActionCodes.AdminRewardCurrency)]
    public class PlayerAdminRewardCurrency : PlayerSynchronizedServerAction
    {
        public int? NewGold { get; private set; }
        public int? NewGems { get; private set; }

        PlayerAdminRewardCurrency() { }
        public PlayerAdminRewardCurrency(int? newGold, int? newGems)
        {
            NewGold = newGold;
            NewGems = newGems;
        }

        public override MetaActionResult Execute(PlayerModel player, bool commit)
        {
            if (commit)
            {
                // Update state
                player.RewardCurrency(NewGold, NewGems);
                PlayerEventBase newEvent  = new PlayerEventAdminRewardCurrency(NewGold, NewGems);
                player.EventStream.Event(newEvent);
                player.Log.Debug(newEvent.EventDescription);
            }

            return ActionResult.Success;
        }
    }
}
