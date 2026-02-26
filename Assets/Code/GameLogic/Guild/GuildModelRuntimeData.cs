// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core.Guild;

namespace Game.Logic
{
    public sealed class GuildModelRuntimeData : GuildModelRuntimeDataBase<GuildModel>
    {
        readonly IGuildModelServerListener _serverListener;
        readonly IGuildModelClientListener _clientListener;

        public GuildModelRuntimeData(GuildModel instance)
            : base(instance)
        {
            _serverListener = instance.ServerListener;
            _clientListener = instance.ClientListener;
        }

        public override void CopySideEffectListenersTo(GuildModel instance)
        {
            base.CopySideEffectListenersTo(instance);

            instance.ServerListener = _serverListener;
            instance.ClientListener = _clientListener;
        }
    };
}
