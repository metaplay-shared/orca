// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Cloud.Entity;
using Metaplay.Core;
using Metaplay.Server;
using System;

namespace Game.Server
{
    [EntityConfig]
    public class SessionConfig : SessionConfigBase
    {
        public override Type EntityActorType => typeof(SessionActor);
    }

    public class SessionActor : SessionActorBase
    {
        public SessionActor(EntityId entityId) : base(entityId)
        {
        }
    }
}
