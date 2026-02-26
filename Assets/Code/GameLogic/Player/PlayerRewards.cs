// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using System.Linq;
using Game.Logic.LiveOpsEvents;
using Metaplay.Core;
using Metaplay.Core.Model;
using Metaplay.Core.Rewards;

namespace Game.Logic
{
    public abstract class PlayerReward : MetaPlayerReward<PlayerModel>
    {
    }

    [MetaSerializableDerived(3)]
    public class RewardCurrency : PlayerReward {
        [MetaMember(1)] public CurrencyTypeId CurrencyId { get; private set; } = CurrencyTypeId.Gold;
        [MetaMember(2)] public int              Amount      { get; private set; } = 1;

        public RewardCurrency() { }
        public RewardCurrency(CurrencyTypeId currencyId, int amount)
        {
            CurrencyId = currencyId;
            Amount = amount;
        }

        public override void Consume(PlayerModel playerModel, IRewardSource source)
        {
            var multiplier = playerModel.LiveOpsEvents.EventModels?.Values.Where(x => x.Phase.IsActivePhase())
                .Select(x => x.Content)
                .OfType<CurrencyMultiplierEvent>()
                .FirstOrDefault(x => x.Type == CurrencyId)
                ?.Multiplier ?? 1;
            playerModel.PurchaseResources(CurrencyId, (int)(Amount * multiplier), IslandTypeId.None, ResourceModificationContext.Empty);
        }
    }

    [MetaSerializableDerived(4)]
    public class PlayerRewardItem : PlayerReward {
        [MetaMember(1)] public LevelId<ChainTypeId> ChainId { get; private set; }
        [MetaMember(2)] public int Amount { get; private set; } = 1;

        public PlayerRewardItem() { }
        public PlayerRewardItem(LevelId<ChainTypeId> chainId, int amount)
        {
            ChainId = chainId;
            Amount = amount;
        }

        public override void Consume(PlayerModel playerModel, IRewardSource source) {
            var islandTypeId = playerModel.GameConfig.Chains[ChainId].TargetIsland;
            for (int i = 0; i < Amount; i++) {
                playerModel.AddItemToHolder(islandTypeId, new ItemModel(ChainId.Type, ChainId.Level, playerModel.GameConfig, MetaTime.Now, true));
            }
        }
    }
}
