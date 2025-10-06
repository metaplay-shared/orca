// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Server.AdminApi;

namespace Game.Server.AdminApi
{
    [AdminApiPermissionGroup("Game-specific permissions")]
    public static class GamePermissions
    {
        /*
        [Permission("An example custom action.")]
        public const string ApiExampleAction = "api.example_action";
        */

        [MetaDescription("Reward player with currency.")]
        [Permission(DefaultRole.GameAdmin)]
        public const string ApiPlayersRewardCurrency = "api.players.reward_currency";
    }
}
