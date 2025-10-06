// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using System.Runtime.Serialization;
using Game.Logic.LiveOpsEvents;
using Game.Logic.TypeCodes;
#if NETCOREAPP
using Metaplay.Cloud.RuntimeOptions;
#endif
using Metaplay.Core.Activables;
using Metaplay.Core.Client;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.Math;
using Metaplay.Core.Offers;
using Metaplay.Core.Rewards;
using Metaplay.Core.Schedule;
using Metaplay.Core.Serialization;

namespace Game.Logic {
	/// <summary>
	/// Class for storing the state and updating the logic for a single player.
	/// </summary>
	[MetaSerializableDerived(1)]
	[SupportedSchemaVersions(1, 2)]
	[MetaReservedMembers(100, 400)]
	public class PlayerModel :
		PlayerModelBase<
			PlayerModel,
			PlayerStatisticsCore,
			PlayerGuildStateCore
		>, ITaskContext {
		public const int TicksPerSecond = 10;
		protected override int GetTicksPerSecond() => TicksPerSecond;

		// External services, not serialized or PrettyPrinted
		[IgnoreDataMember] public new SharedGameConfig						GameConfig => GetGameConfig<SharedGameConfig>();
		[IgnoreDataMember] public IPlayerModelServerListener				ServerListener  { get; set; } = EmptyPlayerModelServerListener.Instance;
		[IgnoreDataMember] public IPlayerModelClientListener				ClientListener  { get; set; } = EmptyPlayerModelClientListener.Instance;

		public override IModelRuntimeData<IPlayerModelBase> GetRuntimeData() => new PlayerModelRuntimeData(this);

		public OrcaDivisionClientState DivisionClientState => PlayerSubClientStates.GetValueOrDefault(ClientSlotGame.OrcaLeague) as OrcaDivisionClientState;

		// Player profile
		[MetaMember(100)] public sealed override EntityId		   PlayerId	{ get; set; }
		[MetaMember(101), NoChecksum] public sealed override string PlayerName  { get; set; }
		[MetaMember(102)] public sealed override int PlayerLevel {
			get => Level.Level;
			set { }
		}
		[MetaMember(103)] public PlayerLevelModel Level { get; private set; }

		[MetaMember(110)] public MetaDictionary<IslandTypeId, IslandModel> Islands { get; private set; }
		[MetaMember(111)] public MergeModel Merge { get; private set; }
		[MetaMember(112)] public PlayerWalletModel Wallet { get; private set; }
		[MetaMember(113)] public InventoryModel Inventory { get; private set; }
		[MetaMember(114)] public HeroesModel Heroes { get; private set; }
		[MetaMember(115)] public List<RewardModel> Rewards { get; private set; }
		[MetaMember(116)] public BackpackModel Backpack { get; private set; }
		[MetaMember(117)] public BuildersModel Builders { get; private set; }
		[MetaMember(118)] public LogbookModel Logbook { get; private set; }
		[MetaMember(130)] public MarketModel Market { get; private set; }

		[MetaMember(140)] public IslandTypeId CurrentIsland { get; set; } = IslandTypeId.MainIsland;
		[MetaMember(141)] public IslandTypeId LastIsland { get; set; } = IslandTypeId.MainIsland;

		[MetaMember(150)] public RandomPCG Random { get; private set; }
		[MetaMember(160)] public long LastFullUpdateTick { get; private set; }

		// Demo
		[MetaMember(170)] public string LatestInfoUrl { get; set; }

		[MetaMember(190)] public TriggerModel Triggers { get; private set; }
		[MetaMember(200)] public PrivateProfile PrivateProfile { get; private set; }

		[MetaMember(300)] public DiscountEventsModel DiscountEvents { get; private set; }
		[MetaMember(310)] public ActivityEventsModel ActivityEvents { get; private set; }
		[MetaMember(320)] public DailyTaskEventsModel DailyTaskEvents { get; private set; }
		[MetaMember(340)] public VipPassesModel VipPasses { get; private set; }
		[MetaMember(341)] public float MatchMakingInterest { get; private set; }
		[MetaMember(342)] public int MatchMakingScore { get; private set; }
		
		[MigrationFromVersion(fromVersion: 1)] 
		void MigrateLeagues()
		{
			// Migrate league client state
#pragma warning disable CS0618 // Type or member is obsolete
			if (PlayerSubClientStates.TryGetValue(ClientSlotCore.PlayerDivisionLegacy, out PlayerSubClientStateBase orcaState))
			{
				PlayerSubClientStates.Remove(ClientSlotCore.PlayerDivisionLegacy);
				PlayerSubClientStates[ClientSlotGame.OrcaLeague] = orcaState;
			}else {
				PlayerSubClientStates[ClientSlotGame.OrcaLeague] = new OrcaDivisionClientState();
			}
#pragma warning restore CS0618 // Type or member is obsolete
		}
		
		/// <summary>
		/// <c>Status</c> returns the status of the given event or <c>null</c> if the event is not in a visible phase.
		/// </summary>
		/// <param name="eventModel">event whose status to check</param>
		/// <returns>status of the event or <c>null</c> if not in visible phase. Note that for an active events
		/// the status can be either <see cref="MetaActivableVisibleStatus.Active"/> or
		/// <see cref="MetaActivableVisibleStatus.EndingSoon"/></returns>
		public MetaActivableVisibleStatus Status(IEventModel eventModel) {
			MetaActivableVisibleStatus status;
			if (eventModel is ActivityEventModel activityEventModel) {
				ActivityEvents.TryGetVisibleStatus(activityEventModel.Info, this, out status);
				return status;
			} else if (eventModel is DiscountEventModel discountEventModel) {
				DiscountEvents.TryGetVisibleStatus(discountEventModel.Info, this, out status);
				return status;
			} else if (eventModel is DailyTaskEventModel dailyTaskEventModel) {
				DailyTaskEvents.TryGetVisibleStatus(dailyTaskEventModel.Info, this, out status);
				return status;
			}

			return null; // Should never happen
		}

		public IEventModel TryGetEventModel(EventId eventId) {
			if (GameConfig.ActivityEvents.ContainsKey(eventId)) {
				return ActivityEvents.TryGetState(GameConfig.ActivityEvents[eventId]);
			}
			if (GameConfig.DiscountEvents.ContainsKey(eventId)) {
				return DiscountEvents.TryGetState(GameConfig.DiscountEvents[eventId]);
			}
			if (GameConfig.DailyTaskEvents.ContainsKey(eventId)) {
				return DailyTaskEvents.TryGetState(GameConfig.DailyTaskEvents[eventId]);
			}
			return null;
		}

		public DailyTaskEventModel GetActiveDailyTaskEventModel() {
			// TODO: Implement more robust resolving of active DailyTaskEventModel
			foreach (DailyTaskEventInfo info in GameConfig.DailyTaskEvents.Values) {
				return DailyTaskEvents.TryGetState(info);
			}

			return null;
		}

		public List<IEventModel> VisibleEventModels() {
			List<IEventModel> eventModels = new List<IEventModel>();
			foreach (ActivityEventInfo info in GameConfig.ActivityEvents.Values) {
				eventModels.Add(ActivityEvents.TryGetState(info));
			}
			foreach (DiscountEventInfo info in GameConfig.DiscountEvents.Values) {
				eventModels.Add(DiscountEvents.TryGetState(info));
			}
			foreach (DailyTaskEventInfo info in GameConfig.DailyTaskEvents.Values) {
				eventModels.Add(DailyTaskEvents.TryGetState(info));
			}

			eventModels.RemoveAll(item => item == null);
			return eventModels.Where(model => model.Status(this) != null).ToList();
		}

		/// <summary>
		/// <c>EnsureEventsHaveStates</c> iterates through events defined in the game config and initializes
		/// any nonexistent models. <see cref="Status"/> and <see cref="VisibleEventModels"/> relies upon
		/// this method to be called at the start of a session. It is also implied that this method should not
		/// be called by the client outside of an action since the method (possibly) modifies the player model.
		/// </summary>
		public void EnsureEventsHaveStates() {
			foreach (ActivityEventInfo info in GameConfig.ActivityEvents.Values) {
				ActivityEvents.SubEnsureHasState(info, this);
			}
			foreach (DiscountEventInfo info in GameConfig.DiscountEvents.Values) {
				DiscountEvents.SubEnsureHasState(info, this);
			}
			foreach (DailyTaskEventInfo info in GameConfig.DailyTaskEvents.Values) {
				DailyTaskEvents.SubEnsureHasState(info, this);
			}
		}

		public void SoftReset() {
			GameInitializeNewPlayerModel(MetaTime.Now, GameConfig, PlayerId, PlayerName);
			InAppPurchaseHistory.Clear();
			LoginHistory.Clear();
			Stats.InitializeForNewPlayer(MetaTime.Now);
			Stats.TotalLogins = 1;
			
			SharedGameConfig gameConfig = (SharedGameConfig) GameConfig;
			foreach (IslandModel island in Islands.Values) {
				island.MergeBoard?.Init();
			}

			Level.AnalyticsEventHandler = e => EventStream.Event(e);
		}
		
		protected override void GameInitializeNewPlayerModel(
			MetaTime now,
			ISharedGameConfig gameConfig,
			EntityId playerId,
			string name
		) {
			PlayerId = playerId;
			PlayerName = GenerateRandomPlayerName(new System.Random());
			Level = new PlayerLevelModel();
			Random = RandomPCG.CreateNew();
			Islands = new MetaDictionary<IslandTypeId, IslandModel>();
			Merge = new MergeModel(GameConfig, now);
			Wallet = new PlayerWalletModel(GameConfig);
			Inventory = new InventoryModel(GameConfig);
			Heroes = new HeroesModel(GameConfig);
			Rewards = new List<RewardModel>();
			Backpack = new BackpackModel(GameConfig);
			Builders = new BuildersModel(GameConfig.Global.BuilderCount);
			Logbook = new LogbookModel();
			Market = new MarketModel();
			Triggers = new TriggerModel();
			PrivateProfile = new PrivateProfile();
			DiscountEvents = new DiscountEventsModel();
			ActivityEvents = new ActivityEventsModel();
			DailyTaskEvents = new DailyTaskEventsModel();
			VipPasses = new VipPassesModel();
			UnlockIslands(false);

			// Initialize division client state
			PlayerSubClientStates.AddIfAbsent(ClientSlotGame.OrcaLeague, new OrcaDivisionClientState());
	
			// some magic number curve
			// https://www.desmos.com/calculator/qnienmzhob
			double a = 20;
			double c = -1.2;
			var x = (playerId.Value % 100 / 100f) - 0.35;
			MatchMakingInterest = Math.Clamp((float)(a * Math.Pow(x, 3) + c * x) + 0.25f, 0, 1);
		}

		static readonly string[] _botNameFormats    = new string[] { "[Wrapper] [Adjective] [Wrapper] [Creature] [Wrapper]", "[Wrapper][Adjective][Creature][Extra][Wrapper]", "[Adjective][Creature]", "[Adjective][Creature][Number]", "[Adjective] [Creature] [Extra]", "[ADJECTIVE] [Extra] [CREATURE]", "[Adjective] [Wrapper] [Creature] [Wrapper] [Number]" };
		static readonly string[] _botNameAdjectives = new string[] { "abashed", "abrasive", "adamant", "admiral", "aloof", "autoritarian", "axiomatic", "barbarous", "beginner", "brittle", "casual", "cranky", "crude", "dapper", "dark", "deceptive", "digital", "draconian", "eager", "easy", "erratic", "fake", "friendly", "genuine", "heady", "helpful", "idle", "insolent", "invisible", "irksome", "irresponsible", "magical", "mannered", "master", "native", "naïve", "nebulous", "obtuse", "professional", "provocative", "rare", "shoddy", "slow", "smart", "sparkling", "special", "spontaneous", "swollen", "talkative", "tenacious", "viral", "weak", "wild", "wise", "witty" };
		static readonly string[] _botNameCreatures  = new string[] { "angel", "ant", "armadillo", "bat", "bear", "bee", "beetle", "blobfish", "boar", "bull", "camel", "chinchilla", "cobra", "cow", "crab", "deer", "dolphin", "dragon", "eagle", "elephant", "flamingo", "fox", "gecko", "giraffe", "goat", "goblin", "grasshopper", "hobbit", "hyena", "kong", "liger", "lizard", "manatee", "mustang", "narwhal", "octopus", "owl", "panda", "pangolin", "parrot", "pegasus", "penguin", "pig", "pigeon", "rabbit", "rat", "raven", "salamander", "salmon", "shark", "sloth", "spider", "squid", "squirrel", "turtle", "unicorn", "wasp", "whale", "wolf", "wolverine", "wombat", "zebra" };
		static readonly string[] _botNameWrappers   = new string[] { "-", "==", "^", "_", "+", "‼️", "❤️", "☠️", "⭐️⭐️", "✨", "⚡️" };
		static readonly string[] _botNameExtras     = new string[] { "🔥", "💯", "🙌", "🚀", "🎩" };

		public static string GenerateRandomPlayerName(Random random)
		{
			string name;
			do
			{
				string selectedFormat = _botNameFormats[random.Next(_botNameFormats.Length)];
				string selectedAdjectiveName = _botNameAdjectives[random.Next(_botNameAdjectives.Length)];
				string selectedCreatureName = _botNameCreatures[random.Next(_botNameCreatures.Length)];
				string selectedWrapper = _botNameWrappers[random.Next(_botNameWrappers.Length)];
				string selectedExtra = _botNameExtras[random.Next(_botNameExtras.Length)];
				string selectedNumber = random.Next(99).ToString(CultureInfo.InvariantCulture.NumberFormat);
				name = "🤖 " + selectedFormat
					.Replace("[Adjective]", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedAdjectiveName))
					.Replace("[ADJECTIVE]", selectedAdjectiveName.ToUpper(CultureInfo.CurrentCulture))
					.Replace("[Creature]", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(selectedCreatureName))
					.Replace("[CREATURE]", selectedCreatureName.ToUpper(CultureInfo.CurrentCulture))
					.Replace("[Extra]", selectedExtra)
					.Replace("[Wrapper]", selectedWrapper)
					.Replace("[Number]", selectedNumber);
			}
			while (name.Length < 5 || name.Length > 20);
			return name;
		}

		protected override void GameOnRestoredFromPersistedState(MetaDuration elapsedTime) {
			PiecewiseUpdate(elapsedTime);
		}

		protected override void GameFastForwardTime(MetaDuration elapsedTime) {
			PiecewiseUpdate(elapsedTime);
		}

		protected override void GameTick(IChecksumContext checksumCtx) {
			Update();
		}

		private int GetTickOnTime(MetaTime time) => (time - TimeAtFirstTick).ToLocalTicks(GetTicksPerSecond());

		/// <summary>
		/// <c>PiecewiseUpdate</c> updates the player model in (possibly) multiple steps. The start and end time of each
		/// step is dictated by the set of "significant" time points within the time span of the update. The significant
		/// times consist of starts and ends of <c>Activables</c> that affect how the model is updated.
		///
		/// NOTE! <c>Update</c> is called both at significant times and 1 second before them. This works around the fact
		/// that an in-game event (activable) is considered active at the start time but not the end time of its
		/// schedule.
		/// </summary>
		/// <param name="elapsedTime">time that has passed since the last update of the model</param>
		private void PiecewiseUpdate(MetaDuration elapsedTime) {
			long originalCurrentTicks = CurrentTick;
			MetaTime originalCurrentTime = CurrentTime;
			MetaTime lastUpdated = CurrentTime - elapsedTime;

			MetaTime? nextSignificantTime = NextSignificantTime(lastUpdated);
			MetaDuration second = MetaDuration.FromSeconds(1);
			while (nextSignificantTime.HasValue && nextSignificantTime < originalCurrentTime) {
				CurrentTick = GetTickOnTime(nextSignificantTime.Value - second);
				Update();
				CurrentTick = GetTickOnTime(nextSignificantTime.Value);
				Update();
				nextSignificantTime = NextSignificantTime(nextSignificantTime.Value);
			}

			CurrentTick = originalCurrentTicks;
			Update();
		}

		/// <summary>
		/// <c>NextSignificantTime</c> calculates the next point in time after <paramref name="startTime"/> that
		/// is "significant" when it comes to how the player model is updated. For example, start and end of an in-game
		/// event that boosts energy production are significant times since they affect how fast the player's energy
		/// is replenished.
		/// </summary>
		/// <param name="startTime">time after which to search for the next significant time</param>
		/// <returns>next significant time or <c>null</c> if none found</returns>
		private MetaTime? NextSignificantTime(MetaTime startTime) {
			MetaTime? next = null;
			foreach (IMetaActivableConfigData<EventId> info in GameConfig.DiscountEvents.Values
						.Union<IMetaActivableConfigData<EventId>>(GameConfig.ActivityEvents.Values)) {
				PlayerLocalTime playerTime = new PlayerLocalTime(startTime, TimeZoneInfo.CurrentUtcOffset);
				MetaScheduleOccasionsQueryResult queryResult = info.ActivableParams.Schedule.QueryOccasions(playerTime);
				MetaScheduleOccasion? occasion = queryResult.CurrentOrNextEnabledOccasion;
				if (occasion.HasValue) {
					MetaTimeRange range = occasion.Value.EnabledRange;
					if (range.Start > startTime && (!next.HasValue || range.Start < next)) {
						next = range.Start;
					} else if (!next.HasValue || range.End < next) {
						next = range.End;
					}
				}
			}

			return next;
		}

		private const int FULL_UPDATE_INTERVAL = 10;
		private void Update() {
			DiscountEvents.TryFinalizeEach(GameConfig.DiscountEvents.Values, player: this);
			DiscountEvents.TryStartActivationForEach(GameConfig.DiscountEvents.Values, player: this);
			ActivityEvents.TryFinalizeEach(GameConfig.ActivityEvents.Values, player: this);
			ActivityEvents.TryStartActivationForEach(GameConfig.ActivityEvents.Values, player: this);
			DailyTaskEvents.TryFinalizeEach(GameConfig.DailyTaskEvents.Values, player: this);
			DailyTaskEvents.TryStartActivationForEach(GameConfig.DailyTaskEvents.Values, player: this);

			Merge.Update(CurrentTime, this.GetCurrentLocalTime(), ClientListener, EnergyGeneratedPerHour, MaxEnergy);
			Heroes.Update(GameConfig, PlayerLevel, Inventory.UnlockedResourceItems, CurrentTime, ClientListener);
			// No need to do this on every tick
			if (CurrentTick - LastFullUpdateTick >= FULL_UPDATE_INTERVAL) {
				foreach (IslandModel island in Islands.Values) {
					island.MergeBoard?.Update(
						Random,
						CurrentTime,
						GameConfig,
						ClientListener,
						MapRewardToRealType,
						HandleItemDiscovery
					);
				}

				LastFullUpdateTick = CurrentTick;
			}

			Market.Update(this);

			int released = Builders.Update(CurrentTime);
			if (released > 0) {
				foreach (IslandModel island in Islands.Values) {
					island.MergeBoard?.FinishBuilders(
						GameConfig,
						Random,
						Builders.OccupiedBuilders,
						ClientListener,
						HandleItemDiscovery
					);
				}
				Builders.Cleanup(GameConfig.Global.BuilderCount, CurrentTime);
				ClientListener.OnBuilderStateChanged();
			}
			foreach (IslandModel island in Islands.Values) {
				island.MergeBoard?.HandleAutoMine(GameConfig, Random, ClientListener);
			}

			VipPasses.Update(CurrentTime, ClientListener);
		
			const int min = 50;
			const int max = 160;
			var midPoint = max * MatchMakingInterest;
			
			MatchMakingScore = min + Random.NextIntMinMax((int)(midPoint - max * .05f), (int)(midPoint + max * .05f));
		}

		[MetaOnDeserialized]
		public void Init(MetaOnDeserializedParams deserializedParams) {
			// TODO: This is a hacky way to get an init method with game config. Fix this when MetaPlay supports such
			// an init properly.
			// NOTE! This method must not have anything that modifies the serialized state!
			SharedGameConfig gameConfig = (SharedGameConfig) deserializedParams.Resolver;
			foreach (IslandModel island in Islands.Values) {
				island.MergeBoard?.Init();
			}

			Level.AnalyticsEventHandler = e => EventStream.Event(e);
		}

		protected override void GameOnSessionStarted() {
			LastFullUpdateTick = CurrentTick;
			foreach (IslandInfo islandInfo in GameConfig.Islands.Values) {
				if (!Islands.ContainsKey(islandInfo.Type)) {
					Islands[islandInfo.Type] = new IslandModel(
						islandInfo,
						GameConfig,
						CurrentTime,
						// This must be empty because the very first item discovery is managed on the server side
						// otherwise. Using the proper handler here will make the very first triggers nonfunctional.
						item => { }
					);
				}
			}

			foreach (IslandModel island in Islands.Values) {
				island.MergeBoard?.ReplaceItems(GameConfig, CurrentTime, ReplacementContextId.Init, ClientListener);
			}

			// Unlock those islands that have not been unlocked because configs were changed at some point and the
			// player has not reached a new level yet.
			UnlockIslands(false);
			Logbook.Refresh(GameConfig, CurrentTime, this, ClientListener);
			EventStream.Event(new PlayerEventWalletBalance(Wallet));

			// Claim rewards for an activity event that the user has participated in the past.
			foreach (ActivityEventInfo info in GameConfig.ActivityEvents.Values) {
				ActivityEventModel activityEvent = ActivityEvents.TryGetState(info);
				if (activityEvent == null) {
					continue;
				}
				MetaActivableVisibleStatus eventStatus = Status(activityEvent);
				if (eventStatus == null || eventStatus is MetaActivableVisibleStatus.InPreview) {
					activityEvent.ClaimRewardsToInbox(this, CurrentTime, ClientListener);
					activityEvent.Terminate();
				}
			}

			Builders.Cleanup(GameConfig.Global.BuilderCount, CurrentTime);

			VipPasses.TryClaimDailyRewards(this);
			UpdateVipPassLockAreas();
		}

		protected override List<MetaOfferGroupInfoBase> GetMetaOfferGroupsToActivate()
		{
			List<MetaOfferGroupInfoBase> offers = new List<MetaOfferGroupInfoBase>();
			foreach ((OfferPlacementId placementId, IEnumerable<MetaOfferGroupInfoBase> offerGroupsInMostImportantFirstOrder) in GameConfig.MetaOfferGroupsPerPlacementInMostImportantFirstOrder)
			{
				foreach (MetaOfferGroupInfoBase offerGroupInfo in offerGroupsInMostImportantFirstOrder)
				{
					if (offerGroupInfo is DefaultMetaOfferGroupInfo defaultMetaOfferGroupInfo && 
					    MetaOfferGroups.CanStartActivation(defaultMetaOfferGroupInfo, player: this) &&
					    MetaOfferGroups.PlacementIsAvailable(player: this, placementId))
					{
						offers.Add(offerGroupInfo);
					}
				}
			}

			return offers;
		}
		
		public override void OnClaimedInAppProduct(
			InAppPurchaseEvent purchaseEvent,
			InAppProductInfoBase productInfoBase,
			out ResolvedPurchaseContentBase resolvedContent
		) {
			InAppProductInfo productInfo = (InAppProductInfo) productInfoBase;
			if (productInfo.Resources.Count > 0 || productInfo.Items.Count > 0) {
				foreach (ResourceInfo resources in productInfo.Resources) {
					var multiplier = LiveOpsEvents.EventModels?.Values.Where(x => x.Phase.IsActivePhase())
							.Select(x => x.Content)
							.OfType<CurrencyMultiplierEvent>()
							.FirstOrDefault(x => x.Type == resources.Type)
							?.Multiplier ?? 1;
					PurchaseResources(resources.Type, (int)(resources.Amount * multiplier), IslandTypeId.None, ResourceModificationContext.Empty);
				}

				foreach (ItemCountInfo itemCount in productInfo.Items) {
					for (int i = 0; i < itemCount.Count; i++) {
						ItemModel item = new ItemModel(itemCount.Type, itemCount.Level, GameConfig, CurrentTime, true);
						IslandTypeId island = item.Info.TargetIsland == IslandTypeId.All
							? IslandTypeId.MainIsland
							: item.Info.TargetIsland;
						AddItemToHolder(island, item);
					}
				}
				ClientListener.OnClaimedInAppProduct(productInfo, purchaseEvent.ReferencePrice);

				resolvedContent = new ResolvedInAppProductContent(productInfo.Resources, productInfo.Items);
			} else if (productInfo.Island != IslandTypeId.None && productInfo.LockArea != null) {
				IslandModel island = Islands[productInfo.Island];
				island.OpenLockArea(
					GameConfig,
					productInfo.LockArea[0],
					HandleItemDiscovery,
					HandleBuildingState,
					ClientListener
				);
				island.RunIslandTaskTriggers(ExecuteTrigger);

				resolvedContent = new ResolvedInAppIslandLockArea(productInfo.Island, productInfo.LockArea);
				ClientListener.OnClaimedInAppProduct(productInfo, purchaseEvent.ReferencePrice);
			} else if (productInfo.VipPassId != VipPassId.None) {
				VipPasses.AddPass(productInfo, this);
				resolvedContent = new ResolvedInAppVipPass(productInfo.VipPassId, productInfo.VipPassDuration);
				ClientListener.OnClaimedInAppProduct(productInfo, purchaseEvent.ReferencePrice);
				// Claim daily reward immediately (without forcing the user to restart their session).
				VipPasses.TryClaimDailyRewards(this);
				UpdateVipPassLockAreas();
			} 
			else if (productInfo.HasDynamicContent)
			{
				resolvedContent = null;
			}
			else {
				throw new Exception("Unknown IAP");
			}
		}

		/// <summary>
		/// <c>UpdateVipPassLockAreas</c> unlocks and locks the lock areas configured in VIP passes according
		/// to the VIP passes currently possessed by the player. NOTE! The implementation assumes that a lock area
		/// is never controlled by more than one VIP pass. In other words, it is a configuration error if two
		/// VIP passes affect the same lock area.
		/// </summary>
		public void UpdateVipPassLockAreas() {
			foreach (VipPassInfo vipPassInfo in GameConfig.VipPasses.Values) {
				foreach (var lockAreaUnlockInfo in vipPassInfo.LockAreaUnlocks) {
					IslandModel island = Islands[lockAreaUnlockInfo.Island];
					MergeBoardModel mergeBoard = island.MergeBoard;
					if (mergeBoard != null) {
						if (VipPasses.HasPass(vipPassInfo.Id)) {
							if (mergeBoard.AreaLockState(lockAreaUnlockInfo.LockAreaIndex) != AreaState.Open) {
								island.OpenLockArea(
									GameConfig,
									lockAreaUnlockInfo.LockAreaIndex,
									HandleItemDiscovery,
									HandleBuildingState,
									ClientListener
								);
								island.RunIslandTaskTriggers(ExecuteTrigger);
							}
						} else {
							// Note, this must use UnlockArea instead of LockArea since LockArea actually locks the
							// area properly. The lock area state Locked means that some prerequisite is not managed,
							// for example a hero is missing or the player level is too low. UnlockArea transfers the
							// state to Opening which means that the final opening action is missing. In this case it
							// is the VIP pass and the player must renew their purchase.
							mergeBoard.LockArea.UnlockArea(lockAreaUnlockInfo.LockAreaIndex);
						}
					}
				}
			}
		}

		public void InitGame() {
			foreach (IslandModel island in Islands.Values) {
				island.MergeBoard?.InitGame(HandleItemDiscovery);
			}
			EnsureEventsHaveStates();
		}

		public void ConsumeResources(CurrencyTypeId type, int amount, ResourceModificationContext context) {
			if (amount <= 0) {
				return;
			}

			if (type == CurrencyTypeId.Energy) {
				Merge.Energy.Consume(amount);
			} else if (type.WalletResource) {
				Wallet.Currency(type).Consume(amount);
				if (type == CurrencyTypeId.Gems) {
					ProgressDailyTask(DailyTaskTypeId.UseGems, amount, context);
				} else if (type == CurrencyTypeId.Gold) {
					ProgressDailyTask(DailyTaskTypeId.UseGold, amount, context);
				}
			} else if (GameConfig.HeroResources.Contains(type)) {
				Inventory.ModifyResources(type, -amount);
			} else {
				throw new Exception("Invalid resource type configured: " + type);
			}

			ClientListener.OnResourcesModified(type, -amount, context);
		}

		public void PurchaseResources(CurrencyTypeId type, int amount, IslandTypeId island, ResourceModificationContext context) {
			if (amount <= 0) {
				return;
			}

			if (type == CurrencyTypeId.Energy) {
				Merge.Energy.Add(amount);
			} else if (type == CurrencyTypeId.Xp) {
				AddXp(amount);
			} else if (type.WalletResource) {
				Wallet.Currency(type).Purchase(amount);
				if (type == CurrencyTypeId.Gold) {
					ProgressDailyTask(DailyTaskTypeId.CollectGold, amount, context);
				}
				foreach (ResourceTriggerInfo triggerInfo in GameConfig.ResourceTriggers.Values) {
					if ((triggerInfo.Island == IslandTypeId.All || triggerInfo.Island == island)
						&& triggerInfo.Currency.WalletResource
						&& Wallet.Currency(triggerInfo.Currency).Value >= triggerInfo.Amount) {
						Triggers.ExecuteTrigger(this, triggerInfo.Trigger);
					}
				}
			} else if (GameConfig.HeroResources.Contains(type)) {
				Inventory.ModifyResources(type, amount);
				ProgressDailyTask(DailyTaskTypeId.CollectResources, amount, context);
				foreach (ResourceTriggerInfo triggerInfo in GameConfig.ResourceTriggers.Values) {
					if ((triggerInfo.Island == IslandTypeId.All || triggerInfo.Island == island)
						&& Inventory.Resources.GetValueOrDefault(triggerInfo.Currency) >= triggerInfo.Amount) {
						Triggers.ExecuteTrigger(this, triggerInfo.Trigger);
					}
				}
			} else {
				throw new Exception("Invalid resource type configured: " + type);
			}
			ClientListener.OnResourcesModified(type, amount, context);
		}
		
		public void EarnResources(CurrencyTypeId type, int amount, IslandTypeId island, ResourceModificationContext context) {
			if (amount <= 0) {
				return;
			}

			if (type == CurrencyTypeId.Energy) {
				Merge.Energy.Add(amount);
			} else if (type == CurrencyTypeId.Xp) {
				AddXp(amount);
			} else if (type.WalletResource) {
				Wallet.Currency(type).Earn(amount);
				if (type == CurrencyTypeId.Gold) {
					ProgressDailyTask(DailyTaskTypeId.CollectGold, amount, context);
				}
				foreach (ResourceTriggerInfo triggerInfo in GameConfig.ResourceTriggers.Values) {
					if ((triggerInfo.Island == IslandTypeId.All || triggerInfo.Island == island)
						&& triggerInfo.Currency.WalletResource
						&& Wallet.Currency(triggerInfo.Currency).Value >= triggerInfo.Amount) {
						Triggers.ExecuteTrigger(this, triggerInfo.Trigger);
					}
				}
			} else if (GameConfig.HeroResources.Contains(type)) {
				Inventory.ModifyResources(type, amount);
				ProgressDailyTask(DailyTaskTypeId.CollectResources, amount, context);
				foreach (ResourceTriggerInfo triggerInfo in GameConfig.ResourceTriggers.Values) {
					if ((triggerInfo.Island == IslandTypeId.All || triggerInfo.Island == island)
						&& Inventory.Resources.GetValueOrDefault(triggerInfo.Currency) >= triggerInfo.Amount) {
						Triggers.ExecuteTrigger(this, triggerInfo.Trigger);
					}
				}
			} else {
				throw new Exception("Invalid resource type configured: " + type);
			}
			ClientListener.OnResourcesModified(type, amount, context);
		}

		private void AddXp(int delta) {
			Level.AddXp(GameConfig, delta, AddReward, ClientListener, ServerListener, ResourceModificationContext.Empty);
			UnlockIslands();
		}

		public void AddReward(RewardModel rewards) {
			if (rewards.Resources.Count > 0 || rewards.Items.Count > 0) {
				Rewards.Add(rewards);
				ClientListener.OnRewardAdded();
			}

			switch (rewards.Metadata.Type) {
				case RewardType.PlayerLevel:
					foreach (TriggerId trigger in GameConfig.PlayerLevels[rewards.Metadata.Level].Triggers) {
						Triggers.ExecuteTrigger(this, trigger);
					}
					break;
				case RewardType.HeroLevel:
				case RewardType.HeroUnlock: // Both cases use the same triggers
					LevelId<HeroTypeId> heroId = new(rewards.Metadata.Hero, rewards.Metadata.Level);
					foreach (TriggerId trigger in GameConfig.HeroLevels[heroId].Triggers) {
						Triggers.ExecuteTrigger(this, trigger);
					}
					break;
				case RewardType.IslandLevel:
					LevelId<IslandTypeId> islandId = new(rewards.Metadata.Island, rewards.Metadata.Level);
					foreach (TriggerId trigger in GameConfig.IslandLevels[islandId].Triggers) {
						Triggers.ExecuteTrigger(this, trigger);
					}
					break;
				case RewardType.BuildingLevel:
					LevelId<IslandTypeId> buildingId = new(rewards.Metadata.Island, rewards.Metadata.Level);
					foreach (TriggerId trigger in GameConfig.BuildingLevels[buildingId].Triggers) {
						Triggers.ExecuteTrigger(this, trigger);
					}
					break;
				case RewardType.ActivityEventLevel:
					LevelId<EventId> activityEventId = new(rewards.Metadata.Event, rewards.Metadata.Level);
					foreach (TriggerId trigger in GameConfig.ActivityEventLevels[activityEventId].Triggers) {
						Triggers.ExecuteTrigger(this, trigger);
					}
					break;
			}
		}

		public void AddActivityEventScore(
			ActivityEventType activityEventType,
			int score,
			ResourceModificationContext context
		) {
			if (score <= 0) {
				return;
			}
			IEnumerable<ActivityEventModel> relevantActivityEventModels =
				ActivityEvents.GetActiveStates(this).Where(model => model.Info.ActivityEventType == activityEventType);
			foreach (ActivityEventModel activityEventModel in relevantActivityEventModels) {
				activityEventModel.AddScore(GameConfig, score, AddReward, ClientListener, ServerListener, context);
			}
		}

		public void ProgressDailyTask(DailyTaskTypeId taskType, int amount, ResourceModificationContext context) {
			foreach (DailyTaskEventModel dailyTaskEvent in DailyTaskEvents.GetActiveStates(this)) {
				dailyTaskEvent.Progress(taskType, amount, ClientListener, context);
			}
		}

		public void HandleItemDiscovery(ItemModel item) {
			if (item.IsDiscovered) {
				return;
			}

			if (item.BuildState == ItemBuildState.Complete) {
				if (Merge.ItemDiscovery.SetDiscovery(item.Info.ConfigKey, CurrentTime)) {
					if (GameConfig.HeroResources.Contains(item.Info.CollectableType) &&
						GameConfig.ResourceItems.ContainsKey(item.Info.CollectableType)) {
						Inventory.UnlockedResourceItems.Add(item.Info.Type);
					}

					ClientListener.OnItemDiscoveryChanged(item.Info.ConfigKey);
					foreach (TriggerId trigger in item.Info.DiscoveredTriggers) {
						Triggers.ExecuteTrigger(this, trigger);
					}
					EventStream.Event(new PlayerItemDiscovered(item.Info.Type, item.Info.Level));
				}

				Logbook.RegisterTaskProgress(
					LogbookTaskType.ItemCount,
					item.Info,
					CurrentTime,
					ClientListener
				);
				Logbook.RegisterTaskProgress(
					LogbookTaskType.ItemDiscovery,
					item.Info,
					CurrentTime,
					ClientListener
				);
				item.Discover(CurrentTime);
			} else if (item.BuildState == ItemBuildState.NotStarted) {
				foreach (TriggerId trigger in item.Info.WaitingBuilderTriggers) {
					Triggers.ExecuteTrigger(this, trigger);
				}
			}
		}

		public void HandleBuildingState(IslandTypeId islandId) {
			IslandModel island = Islands[islandId];
			if (island.BuildingState == BuildingState.Revealed) {
				foreach (TriggerId trigger in island.Info.RevealBuildingTriggers) {
					Triggers.ExecuteTrigger(this, trigger);
				}
			} else if (island.BuildingState == BuildingState.Started) {
				foreach (TriggerId trigger in island.Info.StartBuildingTriggers) {
					Triggers.ExecuteTrigger(this, trigger);
				}
			} else if (island.BuildingState == BuildingState.Complete) {
				island.MergeBoard.MarkBuildingComplete(GameConfig, CurrentTime, ClientListener);
			}
		}

		public void RemoveFromItemHolder(IslandTypeId island, ItemModel item) {
			Islands[island].MergeBoard.ItemHolder.Remove(item);
			ClientListener.OnItemHolderModified(island);
		}

		public void AddItemToHolder(IslandTypeId island, ItemModel item) {
			IslandModel islandModel = Islands[island];
			if (islandModel.MergeBoard == null) {
				// Make sure we don't fail if an item is added to an island that has not been opened yet.
				islandModel.InitMergeBoard(GameConfig, CurrentTime, HandleItemDiscovery);
			}
			islandModel.MergeBoard.ItemHolder.Add(item);
			ClientListener.OnItemHolderModified(island);
			islandModel.MergeBoard.AdjustItemHolder(ClientListener);
		}

		public void UnlockHero() {
			Heroes.UnlockHero(GameConfig, AddReward, ClientListener, CurrentTime);
			if (Heroes.CurrentHero != HeroTypeId.None) {
				LevelId<HeroTypeId> heroId = new(Heroes.CurrentHero, 1);
				HeroLevelInfo levelInfo = GameConfig.HeroLevels[heroId];
				foreach (TriggerId trigger in levelInfo.Triggers) {
					Triggers.ExecuteTrigger(this, trigger);
				}
			}
			UnlockIslands();
		}

		public void UnlockIslands(bool handleDiscovery = true) {
			foreach (IslandModel island in Islands.Values) {
				if (island.State == IslandState.Hidden && island.Info.PlayerLevel <= PlayerLevel &&
					(island.Info.Hero == HeroTypeId.None || Heroes.Heroes.ContainsKey(island.Info.Hero) &&
						Heroes.Heroes[island.Info.Hero].Level.Level >= island.Info.HeroLevel)) {
					IslandState newState = IslandState.Revealing;
					island.ModifyState(
						newState,
						GameConfig,
						CurrentTime,
						// Item discovery is not handled when the state is created since there is no UI to react on it
						// at that point. PlayerInitGameAction will do it a bit later.
						handleDiscovery ? HandleItemDiscovery : _ => { },
						ClientListener
					);
				}

				if (island.State == IslandState.Open) {
					// Update tasks on all islands to make sure that config changes get properly reflected on new game
					// sessions
					island.Tasks?.UpdateTasks(island.Info, GameConfig, ClientListener);
					island.MergeBoard.ManageLockAreas(
						PlayerLevel,
						Heroes.Heroes,
						GameConfig,
						ClientListener
					);
				}
			}
		}

		public Cost SkipHeroTaskTimerCost(HeroTypeId heroType) {
			TimerCostInfo costInfo = GameConfig.TimerCosts[TimerTypeId.SkipHeroTaskTimer];
			MetaDuration timeLeft = Heroes.Heroes[heroType].CurrentTask.FinishedAt - CurrentTime;
			if (timeLeft <= MetaDuration.Zero) {
				return new Cost(costInfo.CurrencyType, 0);
			}
			int cost = costInfo.CalculateCost(F64.CeilToInt(timeLeft.ToSecondsF64()));
			return new Cost(costInfo.CurrencyType, cost);
		}

		public MetaDuration BuildTimeLeft(int builderId) {
			MetaDuration timeLeft = Builders.GetCompleteAt(builderId) - CurrentTime;
			if (timeLeft < MetaDuration.Zero) {
				return MetaDuration.Zero;
			}

			return timeLeft;
		}

		public MetaDuration TotalBuildTime(int builderId) {
			return Builders.GetTotalTime(builderId);
		}

		public F64 BuilderTimerFactor() {
			F64 discountFactor = F64.One;
			IEnumerable<DiscountEventModel> activeDiscountEvents = DiscountEvents.GetActiveStates(this);
			foreach (DiscountEventModel eventModel in activeDiscountEvents) {
				if (eventModel.Info.DiscountEventType == DiscountEventType.BuilderTimer &&
					eventModel.Info.BuilderTimerFactor > 0) {
					discountFactor *= eventModel.Info.BuilderTimerFactor;
				}
			}

			discountFactor *= this.VipPasses.BuilderTimerFactor();
			return discountFactor;
		}

		public int MaxEnergy => GameConfig.Merge.MaxEnergy + VipPasses.MaxEnergyBoost();

		public F64 EnergyGeneratedPerHour {
			get {
				IEnumerable<DiscountEventModel> activeDiscountEvents = DiscountEvents.GetActiveStates(this);
				F64 productionPerHour = GameConfig.Merge.GeneratedPerHour;
				foreach (DiscountEventModel discountEvent in activeDiscountEvents) {
					if (discountEvent.Info.DiscountEventType == DiscountEventType.Energy &&
						discountEvent.Info.EnergyProductionFactor > 0) {
						productionPerHour *= discountEvent.Info.EnergyProductionFactor;
					}
				}

				return productionPerHour * VipPasses.EnergyProductionFactor();
			}
		}

		public Cost SkipBuilderTimerCost(int builderId) {
			bool discountEventOn = DiscountEvents.GetActiveStates(this)
				.Any(e => e.Info.DiscountEventType == DiscountEventType.BuilderTimerGold);
			TimerTypeId timerType = discountEventOn ? TimerTypeId.SkipBuilderTimerGold : TimerTypeId.SkipBuilderTimer;
			TimerCostInfo costInfo = GameConfig.TimerCosts[timerType];
			MetaDuration timeLeft = BuildTimeLeft(builderId);
			if (timeLeft <= MetaDuration.Zero) {
				return new Cost(costInfo.CurrencyType, 0);
			}
			int cost = costInfo.CalculateCost(F64.CeilToInt(timeLeft.ToSecondsF64()));
			return new Cost(costInfo.CurrencyType, cost);
		}

		public ChainTypeId MapRewardToRealTypeUI(IslandTypeId island, ChainTypeId rewardType) {
			return MapRewardToRealType(island, rewardType, RandomPCG.CreateNew());
		}

		/// <summary>
		/// Note, this method MUST NOT be called from the UI code. Calling this method from UI code may cause checksum
		/// mismatches.
		/// </summary>
		/// <param name="island"></param>
		/// <param name="rewardType"></param>
		/// <returns></returns>
		public ChainTypeId MapRewardToRealType(IslandTypeId island, ChainTypeId rewardType) {
			return MapRewardToRealType(island, rewardType, Random);
		}

		// NOTE! When complementing the implementation with additional pseudo item types, add them to the config
		// validation as well. (See SharedGameConfig.BuildTimeValidate())
		private ChainTypeId MapRewardToRealType(IslandTypeId island, ChainTypeId rewardType, RandomPCG random) {
			if (rewardType == ChainTypeId.ResourceItem) {
				return Inventory.GetRandomUnlockedResourceItem(GameConfig, random, island);
			}
			if (rewardType == ChainTypeId.ResourceCreator) {
				return Inventory.GetRandomUnlockedResourceCreator(GameConfig, random, island);
			}
			if (rewardType == ChainTypeId.HeroItem) {
				if (Heroes.CurrentHeroItem == ChainTypeId.None) {
					ItemReplacementInfo replacement = GameConfig.ItemReplacements[new ReplacementId(
						ReplacementContextId.NoNewHero,
						ChainTypeId.HeroItem,
						1
					)];
					return replacement.ReplacementType;
				}
				return Heroes.CurrentHeroItem;
			}
			if (rewardType == ChainTypeId.BuildingItem) {
				List<ChainTypeId> buildingFragments = GameConfig.IslandBuildingFragments[island];
				if (buildingFragments.Count == 0) {
					// Flash sale creation calls this method with building fragments on islands without a building.
					// Those items are cleaned up from the shop list later.
					return ChainTypeId.None;
				}
				return buildingFragments[random.NextInt(buildingFragments.Count)];
			}
			if (rewardType == ChainTypeId.IslandCreator) {
				return Islands[island].Info.IslandCreator;
			}
			if (rewardType == ChainTypeId.IslandChest) {
				return Islands[island].Info.IslandChest;
			}
			return rewardType;
		}

		public void ExecuteTrigger(TriggerId trigger) {
			Triggers.ExecuteTrigger(this, trigger);
		}

		public void UnlockFeature(FeatureTypeId feature) {
			PrivateProfile.FeaturesEnabled.Add(feature);
			if (feature.IsEvent) {
				EnsureEventsHaveStates();
			}
			ClientListener.OnFeatureUnlocked(feature);
		}

		public DiscoveryState GetState(LevelId<ChainTypeId> item) {
			return Merge.ItemDiscovery.GetState(item);
		}

		public IslandState GetState(IslandTypeId island) {
			return Islands[island].State;
		}

		public void RewardCurrency(int? newGold, int? newGems)
		{
			if (newGold.HasValue){
				Wallet.Gold.Earn(newGold.Value);
				ClientListener.OnResourcesModified(CurrencyTypeId.Gold, newGold.Value, new MailResourceContext());
			}

			if (newGems.HasValue) {
				Wallet.Gems.Earn(newGems.Value);
				ClientListener.OnResourcesModified(CurrencyTypeId.Gems, newGems.Value, new MailResourceContext());
			}
		}
	}

	public interface IPlayerModelClientListener {
		void OnItemMovedOnBoard(IslandTypeId islandId, ItemModel item, int fromX, int fromY, int toX, int toY);
		void OnItemCreatedOnBoard(IslandTypeId islandId, ItemModel item, int fromX, int fromY, int toX, int toY, bool spawned);
		void OnItemRemovedFromBoard(IslandTypeId islandId, ItemModel item, int x, int y);
		void OnItemMerged(IslandTypeId island, ItemModel newItem);
		void OnMergeItemStateChanged(IslandTypeId islandId, ItemModel item);
		void OnResourcesModified(CurrencyTypeId resourceType, int diff, ResourceModificationContext context);
		void OnHeroUnlocked(HeroTypeId heroType);
		void OnNewHeroStarted(HeroTypeId heroType);
		void OnHeroTaskModified(HeroTypeId heroType);
		void OnIslandTaskModified(IslandTypeId island, IslanderId islander);
		void OnItemHolderModified(IslandTypeId island);
		void OnIslandStateModified(IslandTypeId island);
		void OnPlayerXpAdded(int delta);
		void OnPlayerLevelUp(RewardModel rewards);
		void OnIslandXpAdded(IslandTypeId island, int delta);
		void OnIslandLevelUp(IslandTypeId island, RewardModel rewards);
		void OnBuildingXpAdded(IslandTypeId island, int delta);
		void OnBuildingLevelUp(IslandTypeId island, RewardModel rewards);
		void OnHeroXpAdded(HeroTypeId hero, int delta);
		void OnHeroLevelUp(HeroTypeId hero, RewardModel rewards);
		void OnBuildingFragmentCollected(IslandTypeId island, ItemModel item, int x, int y);
		void OnBuildingRevealed(IslandTypeId island);
		void OnBuildingCompleted(IslandTypeId island);
		void OnItemTransferredToIsland(IslandTypeId island, ItemModel item, int x, int y);
		void OnRewardAdded();
		void OnRewardClaimed();
		void OnItemDiscoveryChanged(LevelId<ChainTypeId> chainId);
		void OnLockAreaUnlocked(IslandTypeId islandId, char areaIndex);
		void OnLockAreaOpened(IslandTypeId islandId, char areaIndex);
		void OnFeatureUnlocked(FeatureTypeId feature);
		void OnDialogueStarted(DialogueId dialogue);
		void OnHighlightElement(string element);
		void OnHighlightItem(ChainTypeId type, int level);
		void OnPointItem(ChainTypeId type, int level);
		void OnMergeHint(ChainTypeId type1, int level1, ChainTypeId type2, int level2);
		void OnShopUpdated();
		void OnMarketItemUpdated(LevelId<ShopCategoryId> shopItemId);
		void OnBackpackUpgraded();
		void OnItemStoredToBackpack(IslandTypeId island, ItemModel item, int x, int y);
		void OnItemRemovedFromBackpack(int index, ItemModel item);
		void OnIslandRemoved(IslandTypeId island);
		void OnGoToIsland(IslandTypeId island);
		void OnHighlightIsland(IslandTypeId island);
		void OnPointIsland(IslandTypeId island);
		void OnClaimedInAppProduct(InAppProductInfo product, F64 referencePrice);
		void OnBuilderStateChanged();
		void OnBuilderFinished(ItemModel item);
		void OnActivityEventScoreAdded(EventId eventId, int level, int delta, ResourceModificationContext context);
		void OnActivityEventLevelUp(EventId eventId, RewardModel rewards);
		void OnActivityEventPremiumPassBought(EventId eventId);
		void OnHeroMovedToBuilding(HeroTypeId hero, ChainTypeId sourceBuilding, ChainTypeId targetBuilding);
		void OnEventStateChanged(EventId eventId);
		void OnDailyTaskProgressMade(EventId eventId, int progressAmount, ResourceModificationContext context);
		void OnActivityEventRewardClaimed(EventId eventId, int level, bool premium);
		void OnBuilderUsed(IslandTypeId island, ItemModel item, int duration);
		void OnOpenOffer(InAppProductId product);
		void OnVipPassesChanged();
		void OnLogbookTaskModified(LogbookTaskId id);
		void OnLogbookChapterUnlocked(LogbookChapterId id);
		void OnLogbookChapterModified(LogbookChapterId id);
		void OnOpenInfo(string url);
		void OnMergeScoreChanged(int mergeScore);
	}

	public class EmptyPlayerModelClientListener : IPlayerModelClientListener {
		public static readonly EmptyPlayerModelClientListener Instance = new EmptyPlayerModelClientListener();

		public void OnItemMovedOnBoard(IslandTypeId islandId, ItemModel item, int fromX, int fromY, int toX, int toY) { }
		public void OnItemCreatedOnBoard(IslandTypeId islandId, ItemModel item, int fromX, int fromY, int toX, int toY, bool spawned) { }
		public void OnItemRemovedFromBoard(IslandTypeId islandId, ItemModel item, int x, int y) { }
		public void OnItemMerged(IslandTypeId island, ItemModel newItem) { }
		public void OnMergeItemStateChanged(IslandTypeId islandId, ItemModel item) { }
		public void OnResourcesModified(CurrencyTypeId resourceType, int diff, ResourceModificationContext context) { }
		public void OnHeroUnlocked(HeroTypeId heroType) { }
		public void OnNewHeroStarted(HeroTypeId heroType) { }
		public void OnHeroTaskModified(HeroTypeId heroType) { }
		public void OnIslandTaskModified(IslandTypeId island, IslanderId islander) { }
		public void OnItemHolderModified(IslandTypeId island) { }
		public void OnIslandStateModified(IslandTypeId island) { }
		public void OnPlayerXpAdded(int delta) { }
		public void OnPlayerLevelUp(RewardModel rewards) { }
		public void OnIslandXpAdded(IslandTypeId island, int delta) { }
		public void OnIslandLevelUp(IslandTypeId island, RewardModel rewards) { }
		public void OnBuildingXpAdded(IslandTypeId island, int delta) { }
		public void OnBuildingLevelUp(IslandTypeId island, RewardModel rewards) { }
		public void OnHeroXpAdded(HeroTypeId hero, int delta) { }
		public void OnHeroLevelUp(HeroTypeId hero, RewardModel rewards) { }
		public void OnBuildingFragmentCollected(IslandTypeId island, ItemModel item, int x, int y) { }
		public void OnBuildingRevealed(IslandTypeId island) { }
		public void OnBuildingCompleted(IslandTypeId island) { }
		public void OnItemTransferredToIsland(IslandTypeId island, ItemModel item, int x, int y) { }
		public void OnRewardAdded() { }
		public void OnRewardClaimed() { }
		public void OnItemDiscoveryChanged(LevelId<ChainTypeId> chainId) { }
		public void OnLockAreaUnlocked(IslandTypeId islandId, char areaIndex) { }
		public void OnLockAreaOpened(IslandTypeId islandId, char areaIndex) { }
		public void OnFeatureUnlocked(FeatureTypeId feature) {}
		public void OnDialogueStarted(DialogueId dialogue) { }
		public void OnHighlightElement(string element) { }
		public void OnHighlightItem(ChainTypeId type, int level) { }
		public void OnPointItem(ChainTypeId type, int level) {}
		public void OnMergeHint(ChainTypeId type1, int level1, ChainTypeId type2, int level2) { }
		public void OnShopUpdated() { }
		public void OnMarketItemUpdated(LevelId<ShopCategoryId> shopItemId) { }
		public void OnBackpackUpgraded() { }
		public void OnItemStoredToBackpack(IslandTypeId island, ItemModel item, int x, int y) { }
		public void OnItemRemovedFromBackpack(int index, ItemModel item) { }
		public void OnIslandRemoved(IslandTypeId island) { }
		public void OnGoToIsland(IslandTypeId island) { }
		public void OnHighlightIsland(IslandTypeId island) { }
		public void OnPointIsland(IslandTypeId island) { }
		public void OnClaimedInAppProduct(InAppProductInfo product, F64 referencePrice) { }
		public void OnBuilderStateChanged() { }
		public void OnBuilderFinished(ItemModel item) { }
		public void OnActivityEventScoreAdded(EventId eventId, int level, int delta, ResourceModificationContext context) { }
		public void OnActivityEventLevelUp(EventId eventId, RewardModel rewards) { }
		public void OnActivityEventPremiumPassBought(EventId eventId) { }
		public void OnHeroMovedToBuilding(HeroTypeId hero, ChainTypeId sourceBuilding, ChainTypeId targetBuilding) { }
		public void OnEventStateChanged(EventId eventId) { }
		public void OnDailyTaskProgressMade(EventId eventId, int progressAmount, ResourceModificationContext context) { }
		public void OnActivityEventRewardClaimed(EventId eventId, int level, bool premium) { }
		public void OnBuilderUsed(IslandTypeId island, ItemModel item, int duration) { }
		public void OnOpenOffer(InAppProductId product) { }
		public void OnVipPassesChanged() { }
		public void OnLogbookTaskModified(LogbookTaskId id) { }
		public void OnLogbookChapterUnlocked(LogbookChapterId id) { }
		public void OnLogbookChapterModified(LogbookChapterId id) { }
		public void OnOpenInfo(string url) { }
		public void OnMergeScoreChanged(int mergeScore) { }
	}

	public interface IPlayerModelServerListener {
		void OnActivityEventScoreAdded(EventId @event, int level, int delta, ResourceModificationContext context);
		void OnPlayerXpAdded(int delta);
		void OnIslandXpAdded(IslandTypeId island, int delta);
		void OnBuildingXpAdded(IslandTypeId island, int delta);
		void OnHeroXpAdded(HeroTypeId hero, int delta);
		void ItemMerged(ItemModel newItem, int mergeScore);
	}

	public class EmptyPlayerModelServerListener : IPlayerModelServerListener {
		public static readonly EmptyPlayerModelServerListener Instance = new EmptyPlayerModelServerListener();
		public void OnActivityEventScoreAdded(EventId @event, int level, int delta, ResourceModificationContext context) { }

		public void OnPlayerXpAdded(int delta) { }

		public void OnIslandXpAdded(IslandTypeId island, int delta) { }

		public void OnBuildingXpAdded(IslandTypeId island, int delta) { }

		public void OnHeroXpAdded(HeroTypeId hero, int delta) { }
		public void ItemMerged(ItemModel newItem, int mergeScore) {
		}
	}
}
