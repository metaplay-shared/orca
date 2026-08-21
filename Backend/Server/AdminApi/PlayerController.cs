// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Akka.Actor;
using Game.Logic;
using Metaplay.Core.Model;
using Metaplay.Server.AdminApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Server.AdminApi.AuditLog;
using Metaplay.Core.AuditLog;
using Metaplay.Server.AdminApi;
using static System.FormattableString;

namespace Game.Server.AdminApi.Controllers
{
    /// <summary>
    /// Audit log event for setting player wallet from the dashboard.
    /// </summary>
    [MetaSerializableDerived(GameAuditLogEventCodes.RewardCurrency)]
    public class PlayerEventRewardCurrency : PlayerEventPayloadBase
    {
        [MetaMember(1)] public int? NewGold { get; private set; }
        [MetaMember(2)] public int? NewGems { get; private set; }

        PlayerEventRewardCurrency() { }

        public PlayerEventRewardCurrency(int? newGold, int? newGems)
        {
            NewGold = newGold;
            NewGems = newGems;
        }

        override public string EventTitle => "Currency rewarded";
        override public string EventDescription
        {
            get
            {
                List<string> changes = new List<string>();
                changes.Add(Invariant($"Currency sent to player. "));

                if (NewGold.HasValue)
                    changes.Add(Invariant($"Gold += {NewGold.Value}, "));
                else
                    changes.Add(Invariant($"Gold unchanged, "));

                if (NewGems.HasValue)
                    changes.Add(Invariant($"Gems += {NewGems.Value}."));
                else
                    changes.Add(Invariant($"Gems unchanged."));

                return string.Join("", changes);
            }
        }
    }

    /// <summary>
    /// Controller for you game specific routes that deal with an individual player.
    /// Keeping your code in a separate file help us avoid merge conficts in the future!
    /// </summary>
    public class PlayerController : GameAdminApiController
    {
        /// <summary>
        /// HTTP request for setting a player's wallet
        /// </summary>
        public class PlayerRewardCurrencyBody
        {
            [JsonProperty(Required = Required.AllowNull)]
            public int? NewGold { get; private set; }

            [JsonProperty(Required = Required.AllowNull)]
            public int? NewGems { get; private set; }
        }

        // Action to set a player's wallet to the specified values
        [HttpPost("players/{playerIdStr}/rewardCurrency")]     // This defines the URL of this route
        [RequirePermission(GamePermissions.ApiPlayersRewardCurrency)]    // This defines which users are authorized to use the endpoint
        public async Task RewardCurrency(string playerIdStr)           // Remember to ingest the defined route parameters
        {
            // Get player details
            PlayerDetails       playerDetails = await GetPlayerDetailsAsync(playerIdStr);
            PlayerRewardCurrencyBody body          = await ParseBodyAsync<PlayerRewardCurrencyBody>();

            // Invoke the action via the PlayerActor
            await EnqueuePlayerServerActionAsync(playerDetails.PlayerId, new PlayerAdminRewardCurrency(body.NewGold, body.NewGems));

            // Audit log event
            await WriteAuditLogEventAsync(new PlayerEventBuilder(playerDetails.PlayerId, new PlayerEventRewardCurrency(body.NewGold, body.NewGems)));
        }
    }
}
