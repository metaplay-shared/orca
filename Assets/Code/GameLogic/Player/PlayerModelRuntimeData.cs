// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core.Player;

namespace Game.Logic
{
    public sealed class PlayerModelRuntimeData : PlayerModelRuntimeDataBase<PlayerModel>
    {
        readonly IPlayerModelServerListener ServerListener;
        readonly IPlayerModelClientListener ClientListener;

        public PlayerModelRuntimeData(PlayerModel instance) : base(instance)
        {
            this.ServerListener         = instance.ServerListener;
            this.ClientListener         = instance.ClientListener;
        }

        public override void CopySideEffectListenersTo(PlayerModel instance)
        {
            base.CopySideEffectListenersTo(instance);

            instance.ServerListener = this.ServerListener;
            instance.ClientListener = this.ClientListener;
        }
    }
}
