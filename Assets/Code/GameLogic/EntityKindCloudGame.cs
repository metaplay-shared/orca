// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;

namespace Game.Logic
{
    /// <summary>
    /// Registry for game-specific backend-only <see cref="EntityKind"/>s.
    /// For shared code EntityKinds, add them to <c>EntityKindGame</c>
    /// </summary>
    [EntityKindRegistry(30, 50)]
    [EntityKindRegistry(100, 300)]
    public static class EntityKindGame
    {
        public static readonly EntityKind AsyncMatchmaker = EntityKind.FromValue(30);

        //public static readonly EntityKind Example = EntityKind.FromValue(31);
    }
}
