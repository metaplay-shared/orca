using Metaplay.Core;
using Metaplay.Core.Guild;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using System.Runtime.Serialization;

namespace Game.Logic
{
    /// <summary>
    /// Game-specific guild server action class, which attaches all game-specific actions to <see cref="GuildModel"/>.
    /// As a server action, these actions are always created and issued by server. Note that all transaction finalization
    /// action are created by the server and hence can be <c>GuildServerAction</c>s.
    /// </summary>
    /// <remarks>
    /// This is a convenience wrapper for <see cref="GuildActionBase"/>.
    /// See also <seealso cref="GuildClientAction"/>.
    /// </remarks>
    public abstract class GuildServerAction : GuildActionCore<GuildModel>
    {
    }

    /// <summary>
    /// Game-specific guild client action class, which attaches all game-specific actions to <see cref="GuildModel"/>.
    /// This base class denotes the set of actions that the client may enqueue for execution.
    /// </summary>
    /// <remarks>
    /// This is a convenience wrapper for <see cref="GuildClientActionBase"/>.
    /// See also <seealso cref="GuildServerAction"/>.
    /// </remarks>
    public abstract class GuildClientAction : GuildClientActionCore<GuildModel>
    {
    }

    /// <summary>
    /// Example action. Increases target guild member's poke counter.
    /// This is enqueued by client (when player wants to poke someone), so we mark it as GuildClientAction.
    /// </summary>
    [ModelAction(ActionCodes.GuildPokeMember)]
    public class GuildPokeMember : GuildClientAction
    {
        public EntityId TargetPlayerId  { get; private set; }

        public GuildPokeMember() { }
        public GuildPokeMember(EntityId targetPlayerId) { TargetPlayerId = targetPlayerId; }

        public override MetaActionResult Execute(GuildModel guild, bool commit)
        {
            GuildMember invokingMember = guild.Members[InvokingPlayerId];

            if (!guild.Members.TryGetValue(TargetPlayerId, out GuildMember targetMember))
                return MetaActionResult.NoSuchGuildMember;
            // disabled to make testing easier
            //if (TargetPlayerId == InvokingPlayerId)
            //    return GuildActionResult.CannotPokeSelf;

            if (commit)
            {
                targetMember.NumTimesPoked++;

                guild.EventStream.Event(new GuildEventMemberPoked(
                    pokingPlayerId:                 InvokingPlayerId,
                    pokingPlayerMemberInstanceId:   invokingMember.MemberInstanceId,
                    pokingPlayerName:               invokingMember.DisplayName,
                    pokedPlayerId:                  TargetPlayerId,
                    pokedPlayerMemberInstanceId:    targetMember.MemberInstanceId,
                    pokedPlayerName:                targetMember.DisplayName,
                    pokedCountAfter:                targetMember.NumTimesPoked));
            }

            return MetaActionResult.Success;
        }
    }

    // Helper to avoid needing to specify PlayerModel, GuildModel, GuildMember all the time
    /// <summary>
    /// <inheritdoc cref="GuildTransactionBase{TPlayerModel, TGuildModel, TGuildMember, TPlayerPlan, TGuildPlan, TServerPlan, TFinalizingPlan}"/>
    /// </summary>
    public abstract class GuildTransaction<TPlayerPlan,TGuildPlan,TServerPlan,TFinalizingPlan>
        : GuildTransactionBase<PlayerModel, GuildModel, GuildMember, TPlayerPlan,TGuildPlan,TServerPlan,TFinalizingPlan>
        where TPlayerPlan: ITransactionPlan
        where TGuildPlan : ITransactionPlan
        where TServerPlan : ITransactionPlan
        where TFinalizingPlan : ITransactionPlan
    {
    }

    /// <summary>
    /// Example transaction. Selling "Pokes". Reduces player's own poke meter and gives gold in return.
    ///
    /// <para>
    /// We look up how many pokes the player has, we look up what is the configured price. Then if request is valid, we
    /// run an action removing the pokes from guild member, and run an action adding the gold to the player. If request was
    /// invalid, we do nothing.
    /// </para>
    ///
    /// <para>
    /// Working backward from the actions to plans:
    /// </para>
    ///
    /// <para>
    /// Initiating Player Action is intended to prepare player mode in a state the transaction can proceed, and make sure
    /// it can complete the finalizing actions. In this case we don't need a initiating player action since this transaction
    /// completes either by giving player gold, or by doing nothing. A player in any state can complete these actions, hence
    /// Initiating action is No-Op.
    /// </para>
    ///
    /// <para>
    /// Cancelling Player Action is intended to revert changes made in Initiating Player Action. In this case, cancelling
    /// a transaction (i.e. failing a precondition) results in no gold being added to the player, so it is a No-Op action.
    /// </para>
    ///
    /// <para>
    /// Finalizing Player Action adds the gold from successful selling to the player model. For this we need to know
    /// <c>NumGoldGained</c>. Finalizing actions have only two data sources, the Transaction and FinalizingPlan. As the value
    /// is computed at transaction execution time and is not submitted by client, we fetch the value from FinalizingPlan.
    /// </para>
    ///
    /// <para>
    /// Finalizing Guild Action removes the pokes from the player. For this, we need to know how many pokes were consumed.
    /// Again, we can only look either into Transaction or FinalizingPlan. In this case, we just use the value in Transaction
    /// as the value does not need to computed at execution time.
    /// </para>
    ///
    /// <para>
    /// Plans:
    /// </para>
    ///
    /// <para>
    /// As noted, in FinalizingPlan we need <c>NumGoldGained</c>. We know that <c>NumGoldGained = NumPokesSold * NumGoldPerPoke</c>,
    /// and we know that <c>NumPokesSold = transaction.NumPokesAttemptingToSell</c>. This leaves us with <c>NumGoldPerPoke</c> to
    /// be sorted out. We defined per-poke gold price is GameConfig, but to access it we need either Player or Guild plan to inspect
    /// the state and provide the data. In this case we choose <c>GuildPlan</c>.
    /// </para>
    ///
    /// <para>
    /// In GuildPlan, we simply inspect the gold price configuration value, and store it.
    /// </para>
    ///
    /// <para>
    /// Validation:
    /// </para>
    ///
    /// <para>
    /// Now that we have a happy path covered, but we don't yet check whether any the request is valid. While the system automatically
    /// checks for cases like player being kicked just before the transaction, it cannot know whether a transaction is semantically
    /// valid. For that, we need to add transaction specific validation.
    /// </para>
    ///
    /// <para>
    /// First we note that we need to check whether the guild member has any pokes to sell. For this we store <c>NumTimesPoked</c>
    /// in GuildPlan as NumPokesAttemptingToSell, and then check in FinalizingPlan that transaction does not attempt to sell more
    /// pokes than it has. Attempting to do so cancels the transaction. Note that we could check the validity already in <c>GuildPlan</c>
    /// constuctor, but in this case we choose to defer it until FinalizingPlan to keep all validation in one place. This is a stylistic
    /// choice.
    /// </para>
    ///
    /// <para>
    /// We have now bounded the Client supplied <c>Transaction.NumPokesAttemptingToSell</c> from above, but we need to bound it from below
    /// as well. Otherwise a malicious or misbehaving client could "sell" negative amount of pokes, effectively buying pokes. As the price
    /// is not checked to be positive, the player could end up with a negative gold balance. We fix this by checking <c>NumPokesAttemptingToSell</c>
    /// is non-negative.
    /// </para>
    /// </summary>
    public static class GuildSellPokes
    {
        [MetaSerializableDerived(TransactionPlanCodes.GuildSellPokesGuildPlan)]
        public class GuildPlan : ITransactionPlan
        {
            public int NumPokesAvailable;
            public int NumGoldPerPoke;

            public GuildPlan() { }
            public GuildPlan(Transaction transaction, GuildModel guild, GuildMember member)
            {
                // Inspect the maximum limit of pokes for validation purposes. We could validate
                // transaction.NumPokesAttemptingToSell <= guildPlan.NumPokesAvailable, but we have
                // chosen to defer it to FinalizingPlan.
                NumPokesAvailable = member.NumTimesPoked;

                // Look up the price using Guild's current config.
                NumGoldPerPoke = guild.GameConfig.Guild.NumGoldPerSoldPoke;
            }
        }
        [MetaSerializableDerived(TransactionPlanCodes.GuildSellPokesFinalizingPlan)]
        public class FinalizingPlan : ITransactionPlan
        {
            public int NumGoldGained;

            public FinalizingPlan() { }
            public FinalizingPlan(Transaction transaction, ITransactionPlan playerPlan, GuildPlan guildPlan, ITransactionPlan serverPlan)
            {
                // Validate the transaction inputs not illegal. We could do this in any other
                // plan as well, but we have chosen to concentrate validation here.
                if (transaction.NumPokesAttemptingToSell < 0)
                    throw new TransactionPlanningFailure();

                // Validate we are not attempting to sell more pokes than there are
                if (transaction.NumPokesAttemptingToSell > guildPlan.NumPokesAvailable)
                    throw new TransactionPlanningFailure();

                // All good

                NumGoldGained = guildPlan.NumGoldPerPoke * transaction.NumPokesAttemptingToSell;
            }
        }

        [ModelAction(ActionCodes.GuildSellPokesFinalizingPlayerAction)]
        public class FinalizingPlayerAction : PlayerTransactionFinalizingAction
        {
            public int NumGoldGained;

            public FinalizingPlayerAction() { }
            public FinalizingPlayerAction(Transaction transaction, FinalizingPlan finalizingPlan)
            {
                NumGoldGained = finalizingPlan.NumGoldGained;
            }
            public override MetaActionResult Execute(PlayerModel player, bool commit)
            {
                // Add the gold we gained from selling. We don't need to validate inputs since FinalizingPlan has validated them already.
                if (commit)
                    player.Wallet.Gold.Earn(NumGoldGained);
                return MetaActionResult.Success;
            }
        }
        [ModelAction(ActionCodes.GuildSellPokesFinalizingGuildAction)]
        public class FinalizingGuildAction : GuildServerAction
        {
            public int NumPokesConsumed;

            public FinalizingGuildAction() { }
            public FinalizingGuildAction(Transaction transaction, FinalizingPlan finalizingPlan)
            {
                NumPokesConsumed = transaction.NumPokesAttemptingToSell;
            }
            public override MetaActionResult Execute(GuildModel guild, bool commit)
            {
                // Remove the pokes sold. We don't need to validate inputs since FinalizingPlan has validated them already.
                if (commit)
                {
                    if (guild.Members.TryGetValue(InvokingPlayerId, out var member))
                    {
                        member.NumTimesPoked -= NumPokesConsumed;

                        guild.EventStream.Event(new GuildEventMemberSoldPokes(
                            playerId:               InvokingPlayerId,
                            playerMemberInstanceId: member.MemberInstanceId,
                            playerName:             member.DisplayName,
                            numPokesSold:           NumPokesConsumed,
                            pokedCountAfter:        member.NumTimesPoked));
                    }
                }
                return MetaActionResult.Success;
            }
        }

        /// <summary>
        /// Transaction with one argument: NumPokesAttemptingToSell.
        /// Plans (GuildTransaction template Arguments):
        ///    * No GuildPlan, so plain ITransactionPlan
        ///    * Custom GuildPlan
        ///    * No ServerPlan, so plan ITransactionPlan
        ///    * Custom FinalizingPlan
        /// </summary>
        [MetaSerializableDerived(TransactionCodes.GuildSellPokes)]
        public class Transaction : GuildTransaction<ITransactionPlan, GuildPlan, ITransactionPlan, FinalizingPlan>
        {
            // This is not a not a particulary critical operation. In case of an extraordinary conditions, allow losing either Player or Guild modifications or both.
            [IgnoreDataMember] public override GuildTransactionConsistencyMode ConsistencyMode => GuildTransactionConsistencyMode.Relaxed;

            // The arguments of this transaction
            public int NumPokesAttemptingToSell { get; private set; }

            public Transaction() { }
            public Transaction(int numPokesAttemptingToSell)
            {
                NumPokesAttemptingToSell = numPokesAttemptingToSell;
            }

            // Factory boilerplate.
            public override ITransactionPlan PlanForPlayer(PlayerModel player) => null;
            public override GuildPlan PlanForGuild(GuildModel guild, GuildMember member) => new GuildPlan(this, guild, member);
            public override FinalizingPlan PlanForFinalizing(ITransactionPlan playerPlan, GuildPlan guildPlan, ITransactionPlan serverPlan) => new FinalizingPlan(this, playerPlan, guildPlan, serverPlan);
            public override PlayerActionBase CreateInitiatingPlayerAction(ITransactionPlan playerPlan) => null;
            public override PlayerTransactionFinalizingActionBase CreateFinalizingPlayerAction(FinalizingPlan finalizingPlan) => new FinalizingPlayerAction(this, finalizingPlan);
            public override PlayerTransactionFinalizingActionBase CreateCancelingPlayerAction(ITransactionPlan playerPlan) => null;
            public override GuildActionBase CreateFinalizingGuildAction(FinalizingPlan finalizingPlan) => new FinalizingGuildAction(this, finalizingPlan);
        }
    }

    /// <summary>
    /// Example transaction. Takes gold and increases the "vanity" count.
    ///
    /// <para>
    /// We look up how much the vanity costs. We check player has the gold. We then remove that amount of gold from player,
    /// and then increase the vanity count in guild member. In case we cannot buy the vanity points, we do nothing.
    /// </para>
    ///
    /// <para>
    /// As we try to model this, we quickly notice that removing the gold from player only after the transaction is a problem.
    /// As the Finalizing action is expected to always succeed, we either cannot check the player had the neccessary gold to
    /// buy the vanity points, or the gold balance can become negative. While we could check the gold balance in PlayerPlan or
    /// in the Initiating Action, is it not sufficient. As the client controls the player's timeline, it might modify the gold
    /// balance before Finalizing Action is run. Hence we choose an alternative strategy: We take the gold beforehand, and
    /// then if buying does not succeed, refund it.
    /// </para>
    ///
    /// <para>
    /// So: We look up how much the vanity costs, and remove that amount of gold from player if possible. We then increase the vanity count in
    /// guild member if we can. In case we cannot increase  the vanity points, we refund the gold.
    /// </para>
    ///
    /// <para>
    /// As with GuildSellPokes, we model this backwards:
    /// </para>
    ///
    /// <para>
    /// For Initiating Player Action, we want to remove the gold from player. We deliver the amount in <c>PlayerPlan.TotalPrice</c>.
    /// </para>
    ///
    /// <para>
    /// For Cancelling Player Action, we want refund the gold. We use the same value in <c>PlayerPlan.TotalPrice</c>.
    /// </para>
    ///
    /// <para>
    /// For Finalizing Player Action, there is nothing to do. The gold was already removed in initiating action.
    /// </para>
    ///
    /// <para>
    /// For Finalizing Guild Action, we need the incread of Vanity points by using Transaction.NumVanityIncreased.
    /// </para>
    ///
    /// <para>
    /// As we haven't mentioned GuildPlan or FinalizingPlan, we don't need them.
    /// </para>
    /// </summary>
    public static class GuildBuyVanity
    {
        [MetaSerializableDerived(TransactionPlanCodes.GuildBuyVanityPlayerPlan)]
        public class PlayerPlan : ITransactionPlan
        {
            public int TotalPrice;

            public PlayerPlan() { }
            public PlayerPlan(Transaction transaction, PlayerModel player)
            {
                // Validate client data. Must buy at least one vanity point.
                // If we allow were to allow buying negative count, it would lead to ability to sell.
                // We could also check this in InitiatingPlayerAction and return non-Successful result code.
                // In both cases, as seen in IGuildTransaction, this leads to Transaction Abort.

                // Theoretically we could also check this in FinalizingPlan or GuildPlan, if we had one, but that would
                // allow a subtle vulnerability here. If attacker would sell a negative amount, and we would only
                // check it in Finalizing or GuildPlan, we would take the Cancellation path in IGuildTransaction.
                // In that case, we would give the gold in Initiating Action and then remove it in Finalizing Action,
                // but as we already saw in the description, removing resources in Finalizing Action can lead to issues.
                // The player could essentially have temporary gold loans.

                // All in all, there are two potential attacks here.
                // * Buying negative count, leading to ability to sell.
                // * Buying negative count, getting gold beforehand, spending the gold, going into negative balance in the cancellation path

                if (transaction.NumVanityAttemptingToBuy <= 0)
                    throw new TransactionPlanningFailure();

                // Compute the gold price of the transaction.
                // We use the price in the Player's config.
                TotalPrice = transaction.NumVanityAttemptingToBuy * player.GameConfig.Guild.VanityCostNumGold;
            }
        }

        [ModelAction(ActionCodes.GuildBuyVanityInitiatingPlayerAction)]
        public class InitiatingPlayerAction : PlayerAction
        {
            public int TotalPrice;

            public InitiatingPlayerAction() { }
            public InitiatingPlayerAction(Transaction transaction, PlayerPlan playerPlan)
            {
                TotalPrice = playerPlan.TotalPrice;
            }
            public override MetaActionResult Execute(PlayerModel player, bool commit)
            {
                // Check player has enough gold to buy the vanity. Failure here will abort the transaction.
                if (player.Wallet.Gold.Value < TotalPrice)
                    return ActionResult.NotEnoughResources;

                if (commit)
                {
                    // Reserve the gold for purchase.
                    player.Wallet.Gold.Consume(TotalPrice);
                }
                return MetaActionResult.Success;
            }
        }

        [ModelAction(ActionCodes.GuildBuyVanityCancelingPlayerAction)]
        public class CancelingPlayerAction : PlayerTransactionFinalizingAction
        {
            public int NumGoldRefunded;

            public CancelingPlayerAction() { }
            public CancelingPlayerAction(Transaction transaction, PlayerPlan playerPlan)
            {
                NumGoldRefunded = playerPlan.TotalPrice;
            }
            public override MetaActionResult Execute(PlayerModel player, bool commit)
            {
                // Refund the reserved price.
                if (commit)
                    player.Wallet.Gold.Earn(NumGoldRefunded);
                return MetaActionResult.Success;
            }
        }

        [ModelAction(ActionCodes.GuildBuyVanityFinalizingGuildAction)]
        public class FinalizingGuildAction : GuildServerAction
        {
            public int NumVanityIncreased;

            public FinalizingGuildAction() { }
            public FinalizingGuildAction(Transaction transaction, ITransactionPlan finalizingPlan)
            {
                // transaction.NumVanityAttemptingToBuy is validated in PlayerPlan
                NumVanityIncreased = transaction.NumVanityAttemptingToBuy;
            }
            public override MetaActionResult Execute(GuildModel guild, bool commit)
            {
                // Increase the vanity
                if (commit)
                {
                    if (guild.Members.TryGetValue(InvokingPlayerId, out var member))
                    {
                        member.NumVanityPoints += NumVanityIncreased;
                    }
                }
                return MetaActionResult.Success;
            }
        }

        [MetaSerializableDerived(TransactionCodes.GuildBuyVanity)]
        public class Transaction : GuildTransaction<PlayerPlan, ITransactionPlan, ITransactionPlan, ITransactionPlan>
        {
            [IgnoreDataMember] public override GuildTransactionConsistencyMode ConsistencyMode => GuildTransactionConsistencyMode.Relaxed;

            // The arguments of this transaction
            public int NumVanityAttemptingToBuy { get; private set; }

            public Transaction() { }
            public Transaction(int numVanityAttemptingToBuy)
            {
                NumVanityAttemptingToBuy = numVanityAttemptingToBuy;
            }

            // Factory boilerplate.
            public override PlayerPlan PlanForPlayer(PlayerModel player) => new PlayerPlan(this, player);
            public override ITransactionPlan PlanForGuild(GuildModel guild, GuildMember member) => null;
            public override ITransactionPlan PlanForFinalizing(PlayerPlan playerPlan, ITransactionPlan guildPlan, ITransactionPlan serverPlan) => null;
            public override PlayerActionBase CreateInitiatingPlayerAction(PlayerPlan playerPlan) => new InitiatingPlayerAction(this, playerPlan);
            public override PlayerTransactionFinalizingActionBase CreateFinalizingPlayerAction(ITransactionPlan finalizingPlan) => null;
            public override PlayerTransactionFinalizingActionBase CreateCancelingPlayerAction(PlayerPlan playerPlan) => new CancelingPlayerAction(this, playerPlan);
            public override GuildActionBase CreateFinalizingGuildAction(ITransactionPlan finalizingPlan) => new FinalizingGuildAction(this, finalizingPlan);
        }
    }

    /// <summary>
    /// Example transaction. Based on "vanity" count, adds next unlocked vanity rank reward for player.
    ///
    /// <para>
    /// We look if the guild member has a rank unlocked and take it's rewards. We then give the rewards to the player and
    /// increase the consumed ranke counter in guild member. In case there is nothing to redeem, we do nothing.
    /// </para>
    /// </summary>
    public static class GuildClaimVanityRankReward
    {
        [MetaSerializableDerived(TransactionPlanCodes.GuildClaimVanityRankRewardGuildPlan)]
        public class GuildPlan : ITransactionPlan
        {
            public int RewardGems;
            public int RewardGold;

            public GuildPlan() { }
            public GuildPlan(Transaction transaction, GuildModel guild, GuildMember member)
            {
                // Find how many ranks have been unlocked with the vanity count we have
                int numRanksUnlocked;
                for (numRanksUnlocked = 0; numRanksUnlocked < guild.GameConfig.Guild.VanityRankThresholds.Length; ++numRanksUnlocked)
                {
                    if (member.NumVanityPoints < guild.GameConfig.Guild.VanityRankThresholds[numRanksUnlocked])
                        break;
                }

                // If we have nothing unlocked, cancel.
                if (numRanksUnlocked <= member.NumVanityRanksConsumed)
                    throw new TransactionPlanningFailure();

                // Look up the rewards for the next (this) reward
                RewardGold = guild.GameConfig.Guild.VanityRankRewardGold[member.NumVanityRanksConsumed];
                RewardGems = guild.GameConfig.Guild.VanityRankRewardGems[member.NumVanityRanksConsumed];
            }
        }
        [MetaSerializableDerived(TransactionPlanCodes.GuildClaimVanityRankRewardFinalizingPlan)]
        public class FinalizingPlan : ITransactionPlan
        {
            public int NumRewardGems;
            public int NumRewardGold;

            public FinalizingPlan() { }
            public FinalizingPlan(Transaction transaction, ITransactionPlan playerPlan, GuildPlan guildPlan, ITransactionPlan serverPlan)
            {
                // Just passing thru.
                NumRewardGems = guildPlan.RewardGems;
                NumRewardGold = guildPlan.RewardGold;
            }
        }

        [ModelAction(ActionCodes.GuildClaimVanityRankRewardFinalizingPlayerAction)]
        public class FinalizingPlayerAction : PlayerTransactionFinalizingAction
        {
            public int NumRewardGems;
            public int NumRewardGold;

            public FinalizingPlayerAction() { }
            public FinalizingPlayerAction(Transaction transaction, FinalizingPlan finalizingPlan)
            {
                NumRewardGems = finalizingPlan.NumRewardGems;
                NumRewardGold = finalizingPlan.NumRewardGold;
            }
            public override MetaActionResult Execute(PlayerModel player, bool commit)
            {
                // Give the reward to the player
                if (commit)
                {
                    player.Wallet.Gems.Earn(NumRewardGems);
                    player.Wallet.Gold.Earn(NumRewardGold);
                }
                return MetaActionResult.Success;
            }
        }
        [ModelAction(ActionCodes.GuildClaimVanityRankRewardFinalizingGuildAction)]
        public class FinalizingGuildAction : GuildServerAction
        {
            public FinalizingGuildAction() { }
            public FinalizingGuildAction(Transaction transaction, FinalizingPlan finalizingPlan)
            {
            }
            public override MetaActionResult Execute(GuildModel guild, bool commit)
            {
                // Mark the rank as consumed.
                if (commit)
                {
                    if (guild.Members.TryGetValue(InvokingPlayerId, out var member))
                    {
                        member.NumVanityRanksConsumed += 1;
                    }
                }
                return MetaActionResult.Success;
            }
        }

        [MetaSerializableDerived(TransactionCodes.GuildClaimVanityRankReward)]
        public class Transaction : GuildTransaction<ITransactionPlan, GuildPlan, ITransactionPlan, FinalizingPlan>
        {
            // Example of a very critical operation. It would feel really bad for the user if guild changes got committed but player changes did not. If
            // that happened, player would have lost the particular rank rewards, and would not be able to complete it again since guild recorded it as consumed.
            [IgnoreDataMember] public override GuildTransactionConsistencyMode ConsistencyMode => GuildTransactionConsistencyMode.EventuallyConsistent;

            public Transaction() { }

            public override ITransactionPlan PlanForPlayer(PlayerModel player) => null;
            public override GuildPlan PlanForGuild(GuildModel guild, GuildMember member) => new GuildPlan(this, guild, member);
            public override FinalizingPlan PlanForFinalizing(ITransactionPlan playerPlan, GuildPlan guildPlan, ITransactionPlan serverPlan) => new FinalizingPlan(this, playerPlan, guildPlan, serverPlan);
            public override PlayerActionBase CreateInitiatingPlayerAction(ITransactionPlan playerPlan) => null;
            public override PlayerTransactionFinalizingActionBase CreateFinalizingPlayerAction(FinalizingPlan finalizingPlan) => new FinalizingPlayerAction(this, finalizingPlan);
            public override PlayerTransactionFinalizingActionBase CreateCancelingPlayerAction(ITransactionPlan playerPlan) => null;
            public override GuildActionBase CreateFinalizingGuildAction(FinalizingPlan finalizingPlan) => new FinalizingGuildAction(this, finalizingPlan);
        }
    }
}
