// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core.Client;
using Metaplay.Core.Model;

namespace Game.Logic.TypeCodes
{
    [MetaSerializable]
    public class ClientSlotGame : ClientSlot
    {
        public ClientSlotGame(int id, string name) : base(id, name) { }

        public static readonly ClientSlot Matchmaker = new ClientSlotGame(11, nameof(Matchmaker));
        public static readonly ClientSlot OrcaLeague = new ClientSlotGame(12, nameof(OrcaLeague));

        // Add any game-specific client slots here...

        // public static readonly ClientSlot PvpBattle = new ClientSlotGame(12, nameof(PvpBattle));
    }
}

