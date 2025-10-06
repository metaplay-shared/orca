// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using System;
using static System.FormattableString;

namespace Game.Logic
{
    [MetaSerializableDerived(1)]
    public class PlayerSegmentInfo : PlayerSegmentInfoBase
    {
        public PlayerSegmentInfo(){ }
        public PlayerSegmentInfo(PlayerSegmentId segmentId, PlayerCondition playerCondition, string displayName, string description)
            : base(segmentId, playerCondition, displayName, description)
        {
        }
    }

    public class PlayerSegmentInfoSourceItem : PlayerSegmentBasicInfoSourceItemBase<PlayerSegmentInfo>
    {
        protected override PlayerSegmentInfo CreateSegmentInfo(PlayerSegmentId segmentId, PlayerSegmentBasicCondition playerCondition, string displayName, string description)
        {
            return new PlayerSegmentInfo(segmentId, playerCondition, displayName, description);
        }
    }

    [MetaSerializableDerived(1)]
    public class PlayerPropertyIdGemsPurchased : TypedPlayerPropertyId<int>
    {
        public override int GetTypedValueForPlayer(IPlayerModelBase player) => ((PlayerModel)player).Wallet.Gems.Value;
        public override string DisplayName => "Gems Purchased";
    }

    [MetaSerializableDerived(2)]
    public class PlayerPropertyIdGoldPurchased : TypedPlayerPropertyId<int>
    {
        public override int GetTypedValueForPlayer(IPlayerModelBase player) => ((PlayerModel)player).Wallet.Gold.Value;
        public override string DisplayName => "Gold Purchased";
    }

    [MetaSerializableDerived(3)]
    public class PlayerPropertyIdIslandOpen : TypedPlayerPropertyId<bool>
    {
        [MetaMember(1)] public MetaRef<IslandInfo> IslandType { get; private set; }

        PlayerPropertyIdIslandOpen(){ }
        public PlayerPropertyIdIslandOpen(MetaRef<IslandInfo> islandType)
        {
            IslandType = islandType ?? throw new ArgumentNullException(nameof(islandType));
        }

        public override bool GetTypedValueForPlayer(IPlayerModelBase player)
        {
            if (((PlayerModel)player).Islands.TryGetValue(IslandType.Ref.Type, out IslandModel island))
                return island.State == IslandState.Open;

            return false;
        }
        public override string DisplayName => $"{(IslandType.IsResolved ? IslandType.Ref.Type : Util.ObjectToStringInvariant(IslandType.KeyObject))} open";
        public override string ToString() => Invariant($"{nameof(PlayerPropertyIdIslandOpen)}({IslandType.KeyObject})");
    }

    [MetaSerializableDerived(4)]
    public class PlayerPropertyLastKnownCountry : TypedPlayerPropertyId<string>
    {
        public override string GetTypedValueForPlayer(IPlayerModelBase player) => player.LastKnownLocation?.Country.IsoCode;
        public override string DisplayName => $"Last known country";
    }

    [MetaSerializableDerived(5)]
    public class PlayerPropertyAccountCreatedAt : TypedPlayerPropertyId<MetaTime>
    {
        public override MetaTime GetTypedValueForPlayer(IPlayerModelBase player) => player.Stats.CreatedAt;
        public override string DisplayName => $"Account creation time";
    }

    [MetaSerializableDerived(6)]
    public class PlayerPropertyAccountAge : TypedPlayerPropertyId<MetaDuration>
    {
        public override MetaDuration GetTypedValueForPlayer(IPlayerModelBase player) => player.CurrentTime - player.Stats.CreatedAt;
        public override string DisplayName => $"Account age";
    }

    [MetaSerializableDerived(7)]
    public class PlayerPropertyTimeSinceLastLogin : TypedPlayerPropertyId<MetaDuration>
    {
        public override MetaDuration GetTypedValueForPlayer(IPlayerModelBase player) => player.CurrentTime - player.Stats.LastLoginAt;
        public override string DisplayName => $"Time since last login";
    }
}
