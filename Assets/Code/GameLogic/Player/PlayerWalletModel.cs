using System;
using System.Runtime.Serialization;

using Metaplay.Core.Model;

namespace Game.Logic {
    [MetaSerializable]
    public class PlayerWalletModel {
        [MetaMember(1)] public CurrencyModel Gems { get; set; }
        [MetaMember(2)] public CurrencyModel Gold { get; set; }
        [MetaMember(3)] public CurrencyModel IslandTokens { get; set; }
        [MetaMember(4)] public CurrencyModel TrophyTokens { get; set; }
        [MetaMember(5)] public bool HasOpenedShop { get; set; }
        [MetaMember(6)] public bool HasClosedShop { get; set; }

        public PlayerWalletModel() {
        }

        public PlayerWalletModel(SharedGameConfig gameConfig) {
            Gems = new CurrencyModel(0, gameConfig.Global.InitialGems);
            Gold = new CurrencyModel(0, gameConfig.Global.InitialGold);
            IslandTokens = new CurrencyModel(0, gameConfig.Global.InitialIslandTokens);
            TrophyTokens = new CurrencyModel(0, gameConfig.Global.InitialTrophyTokens);
            HasOpenedShop = false;
            HasClosedShop = false;
        }

        public CurrencyModel Currency(CurrencyTypeId currencyType) {
            if (currencyType == CurrencyTypeId.Gold) {
                return Gold;
            }
            if (currencyType == CurrencyTypeId.Gems) {
                return Gems;
            }
            if (currencyType == CurrencyTypeId.IslandTokens) {
                return IslandTokens;
            }
            if (currencyType == CurrencyTypeId.TrophyTokens) {
                return TrophyTokens;
            }

            throw new ArgumentException("Invalid currency type " + currencyType);
        }

        public bool EnoughCurrency(CurrencyTypeId costType, int value) {
            return value <= Currency(costType).Value;
        }
    }

    /**
     * Currency is stored into two variables - Purchased and Earned - for analytics reasons. We can see the total
     * balance of currency that was originally purchased with real money from the system by using this approach. Note,
     * purchased currency is always spent before the earned one (rewards etc).
     */
    [MetaSerializable]
    public class CurrencyModel {
        [MetaMember(1)] public int Purchased { get; set; }
        [MetaMember(2)] public int Earned { get; set; }
        [IgnoreDataMember] public int Value => Purchased + Earned;

        public CurrencyModel() {
        }

        public CurrencyModel(int purchased, int earned) {
            Purchased = purchased;
            Earned = earned;
        }

        public void Earn(int amount) {
            Earned += amount;
        }

        public void Purchase(int amount) {
            Purchased += amount;
        }

        public void Consume(int amount) {
            if (amount <= Purchased) {
                Purchased -= amount;
            }
            else {
                Earned -= amount - Purchased;
                Purchased = 0;
            }
        }
    }
}
