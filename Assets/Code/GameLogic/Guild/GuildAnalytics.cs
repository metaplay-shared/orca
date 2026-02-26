// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.Analytics;
using Metaplay.Core.Guild;
using Metaplay.Core.Model;
using static System.FormattableString;

namespace Game.Logic
{
    // GAME-SPECIFIC GUILD EVENTS

    public static class GuildEventCodes
    {
        public const int MemberPoked        = 100;
        public const int MemberSoldPokes    = 101;
    }

    [AnalyticsEvent(GuildEventCodes.MemberPoked)]
    public class GuildEventMemberPoked : GuildEventBase
    {
        [MetaMember(1)] public EntityId PokingPlayerId                  { get; private set; }
        [MetaMember(2)] public int      PokingPlayerMemberInstanceId    { get; private set; }
        [MetaMember(3)] public string   PokingPlayerName                { get; private set; }
        [MetaMember(4)] public EntityId PokedPlayerId                   { get; private set; }
        [MetaMember(5)] public int      PokedPlayerMemberInstanceId     { get; private set; }
        [MetaMember(6)] public string   PokedPlayerName                 { get; private set; }
        [MetaMember(7)] public int      PokedCountAfter                 { get; private set; }

        GuildEventMemberPoked(){ }
        public GuildEventMemberPoked(EntityId pokingPlayerId, int pokingPlayerMemberInstanceId, string pokingPlayerName, EntityId pokedPlayerId, int pokedPlayerMemberInstanceId, string pokedPlayerName, int pokedCountAfter)
        {
            PokingPlayerId = pokingPlayerId;
            PokingPlayerMemberInstanceId = pokingPlayerMemberInstanceId;
            PokingPlayerName = pokingPlayerName;
            PokedPlayerId = pokedPlayerId;
            PokedPlayerMemberInstanceId = pokedPlayerMemberInstanceId;
            PokedPlayerName = pokedPlayerName;
            PokedCountAfter = pokedCountAfter;
        }

        public override string EventDescription => Invariant($"{PokingPlayerName} ({PokingPlayerId}, instance {PokingPlayerMemberInstanceId}) poked {PokedPlayerName} ({PokedPlayerId}, instance {PokedPlayerMemberInstanceId}), poke count {PokedCountAfter - 1} -> {PokedCountAfter}.");
    }

    [AnalyticsEvent(GuildEventCodes.MemberSoldPokes)]
    public class GuildEventMemberSoldPokes : GuildEventBase
    {
        [MetaMember(1)] public EntityId PlayerId                { get; private set; }
        [MetaMember(2)] public int      PlayerMemberInstanceId  { get; private set; }
        [MetaMember(3)] public string   PlayerName              { get; private set; }
        [MetaMember(4)] public int      NumPokesSold            { get; private set; }
        [MetaMember(5)] public int      PokedCountAfter         { get; private set; }

        GuildEventMemberSoldPokes(){ }
        public GuildEventMemberSoldPokes(EntityId playerId, int playerMemberInstanceId, string playerName, int numPokesSold, int pokedCountAfter)
        {
            PlayerId = playerId;
            PlayerMemberInstanceId = playerMemberInstanceId;
            PlayerName = playerName;
            NumPokesSold = numPokesSold;
            PokedCountAfter = pokedCountAfter;
        }

        public override string EventDescription => Invariant($"{PlayerName} ({PlayerId}, instance {PlayerMemberInstanceId}) sold {NumPokesSold} poke(s), has {PokedCountAfter} left.");
    }
}
