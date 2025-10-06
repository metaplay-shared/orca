using System;
using Metaplay.Core.Config;
using Metaplay.Core.Model;
using System.Collections.Generic;
using System.Linq;
using Game.Logic.LiveOpsEvents;
using Metaplay.Core;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Localization;
using Metaplay.Core.Offers;
using Metaplay.Core.Player;

namespace Game.Logic {
	/// <summary>
	/// Game-specific global configuration data, visible to both client and server.
	/// </summary>
	[MetaSerializable]
	public class SharedGlobalConfig : GameConfigKeyValue<SharedGlobalConfig> {
		[MetaMember(1)] public int InitialGems { get; private set; }
		[MetaMember(2)] public int InitialGold { get; private set; }
		[MetaMember(3)] public int InitialIslandTokens { get; private set; }
		[MetaMember(4)] public int InitialTrophyTokens { get; private set; }
		[MetaMember(5)] public List<ResourceInfo> InitialResources { get; private set; }
		[MetaMember(6)] public int BuilderCount { get; private set; }
		[MetaMember(7)] public MetaDuration BubbleTtl { get; private set; }
		[MetaMember(10)] public LevelId<ChainTypeId> MergeBubbleSpawn { get; private set; }
		[MetaMember(11)] public int MinLevelForBubbles { get; private set; }
		[MetaMember(12)] public CategoryId DefaultDiscoveryCategory { get; private set; }
		[MetaMember(13)] public bool TriggersEnabled { get; private set; }
		[MetaMember(14)] public bool FastAutoSpawn { get; private set; }
		[MetaMember(15)] public MetaDuration GoldenHeroTaskTtl { get; private set; }
		[MetaMember(16)] public int MaxHeroesInBuilding { get; private set; }
		[MetaMember(17)] public MetaDuration BuildingDailyRewardInterval { get; private set; }
		[MetaMember(18)] public InAppProductId MinOfferedGemProduct { get; private set; }
		[MetaMember(19)] public bool ConfirmGemSpend { get; private set; }
		[MetaMember(20)] public List<CategoryId> DiscoveryCategories { get; private set; }
	}

	/// <summary>
	/// Game-specific shared configuration data between the client and the server.
	/// </summary>
	[GameConfigSyntaxAdapter(headerPrefixReplaces: new string[] { "! -> //" })]
	public class SharedGameConfig : SharedGameConfigBase {

#region Metaplay SDK integrations
		[GameConfigEntry("Languages")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "LanguageId -> LanguageId #key" })]
		public GameConfigLibrary<LanguageId, LanguageInfo> Languages { get; private set; }

        [GameConfigEntry("InAppProducts")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "ProductId -> ProductId #key" })]
        public GameConfigLibrary<InAppProductId, InAppProductInfo> InAppProducts { get; private set; }

        [GameConfigEntry("PlayerSegments")]
		[GameConfigEntryTransform(typeof(PlayerSegmentInfoSourceItem))]
        public GameConfigLibrary<PlayerSegmentId, PlayerSegmentInfo> PlayerSegments { get; private set; }
#endregion

		[GameConfigEntry("Global")]
		public SharedGlobalConfig Global { get; protected set; }

		[GameConfigEntry("Client")]
		public ClientInfo Client { get; protected set; }

		[GameConfigEntry("Guild")]
		public GuildInfo Guild { get; protected set; }

		[GameConfigEntry("Merge")]
		public MergeInfo Merge { get; protected set; }

		[GameConfigEntry("PlayerLevels")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Level -> Level #key" })]
		public GameConfigLibrary<int, PlayerLevelInfo> PlayerLevels { get; protected set; }

		[GameConfigEntry("Islands")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key" })]
		public GameConfigLibrary<IslandTypeId, IslandInfo> Islands { get; protected set; }

		[GameConfigEntry("IslandLevels")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<IslandTypeId>, IslandLevelInfo> IslandLevels { get; protected set; }

		[GameConfigEntry("BuildingLevels")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<IslandTypeId>, BuildingLevelInfo> BuildingLevels { get; protected set; }

		[GameConfigEntry("BuildingFragments")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Island -> Island #key", "Index -> Index #key" })]
		public GameConfigLibrary<LevelId<IslandTypeId>, BuildingFragmentInfo> BuildingFragments { get; protected set; }

		[GameConfigEntry("HeroLevels")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<HeroTypeId>, HeroLevelInfo> HeroLevels { get; protected set; }

		[GameConfigEntry("Heroes")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key" })]
		public GameConfigLibrary<HeroTypeId, HeroInfo> Heroes { get; protected set; }

		[GameConfigEntry("HeroTasks")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Id -> Id #key" })]
		public GameConfigLibrary<int, HeroTaskInfo> HeroTasks { get; protected set; }

		[GameConfigEntry("Chains")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<ChainTypeId>, ChainInfo> Chains { get; protected set; }

		[GameConfigEntry("Creators")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<CreatorTypeId>, CreatorInfo> Creators { get; protected set; }

		[GameConfigEntry("Converters")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<ConverterTypeId>, ConverterInfo> Converters { get; protected set; }

		[GameConfigEntry("Mines")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<MineTypeId>, MineInfo> Mines { get; protected set; }

		[GameConfigEntry("Boosters")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key" })]
		public GameConfigLibrary<BoosterTypeId, BoosterInfo> Boosters { get; protected set; }

		[GameConfigEntry("TimerCosts")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Type -> Type #key" })]
		public GameConfigLibrary<TimerTypeId, TimerCostInfo> TimerCosts { get; protected set; }

		[GameConfigEntry("EnergyCosts")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Index -> Index #key" })]
		public GameConfigLibrary<int, EnergyCostInfo> EnergyCosts { get; protected set; }

		[GameConfigEntry("AssignHeroCosts")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Index -> Index #key" })]
		public GameConfigLibrary<int, AssignHeroCostInfo> AssignHeroCosts { get; protected set; }

		[GameConfigEntry("LockAreas")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "IslandId -> IslandId #key", "Index -> Index #key" })]
		public GameConfigLibrary<LockAreaId, LockAreaInfo> LockAreas { get; protected set; }

		[GameConfigEntry("InitialItems")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "IslandId -> IslandId #key", "X -> X #key", "Y -> Y #key" })]
		public GameConfigLibrary<IslandCoordinate, InitialItemInfo> InitialItems { get; protected set; }

		[GameConfigEntry("IslandTasks")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Islander -> Islander #key", "Id -> Id #key" })]
		public GameConfigLibrary<LevelId<IslanderId>, IslandTaskInfo> IslandTasks { get; protected set; }

		[GameConfigEntry("LogbookChapters")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Id -> Id #key" })]
		public GameConfigLibrary<LogbookChapterId, LogbookChapterInfo> LogbookChapters { get; protected set; }

		[GameConfigEntry("LogbookTasks")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Id -> Id #key" })]
		public GameConfigLibrary<LogbookTaskId, LogbookTaskInfo> LogbookTasks { get; protected set; }

		[GameConfigEntry("Shop")]
		public ShopInfo Shop { get; protected set; }

		[GameConfigEntry("VipPasses")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Id -> Id #key" })]
		public GameConfigLibrary<VipPassId, VipPassInfo> VipPasses { get; protected set; }

		[GameConfigEntry("MarketItems")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Category -> Category #key", "Index -> Index #key" })]
		public GameConfigLibrary<LevelId<ShopCategoryId>, MarketItemInfo> MarketItems { get; protected set; }

		[GameConfigEntry("Triggers")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Trigger -> Trigger #key" })]
		public GameConfigLibrary<TriggerId, TriggerInfo> Triggers { get; protected set; }

		[GameConfigEntry("ResourceTriggers")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Island -> Island #key", "Currency -> Currency #key", "Amount -> Amount #key" })]
		public GameConfigLibrary<ResourceTriggerId, ResourceTriggerInfo> ResourceTriggers { get; protected set; }

		[GameConfigEntry("MapTriggers")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Trigger -> Trigger #key" })]
		public GameConfigLibrary<TriggerId, MapTriggerInfo> MapTriggers { get; protected set; }

		[GameConfigEntry("Dialogues")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Id -> Id #key" })]
		public GameConfigLibrary<DialogueId, DialogueInfo> Dialogues { get; private set; }

		[GameConfigEntry("BackpackLevels")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Level -> Level #key" })]
		public GameConfigLibrary<int, BackpackLevelInfo> BackpackLevels { get; private set; }

		[GameConfigEntry("ItemReplacements")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "Context -> Context #key", "Type -> Type #key", "Level -> Level #key" })]
		public GameConfigLibrary<ReplacementId, ItemReplacementInfo> ItemReplacements { get; private set; }

		[GameConfigEntry("DiscountEvents")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "EventId -> EventId #key" })]
		[GameConfigSyntaxAdapter(headerPrefixReplaces: new string[]{ "# -> Schedule." }, headerReplaces: new string[]{ "#StartDate -> Schedule.Start.Date", "#StartTime -> Schedule.Start.Time" })]
		[GameConfigEntryTransform(typeof(DiscountEventConfigItem))]
		public GameConfigLibrary<EventId, DiscountEventInfo> DiscountEvents { get; private set; }

		[GameConfigEntry("ActivityEvents")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "EventId -> EventId #key" })]
		[GameConfigSyntaxAdapter(headerPrefixReplaces: new string[]{ "# -> Schedule." }, headerReplaces: new string[]{ "#StartDate -> Schedule.Start.Date", "#StartTime -> Schedule.Start.Time" })]
		[GameConfigEntryTransform(typeof(ActivityEventConfigItem))]
		public GameConfigLibrary<EventId, ActivityEventInfo> ActivityEvents { get; private set; }

		[GameConfigEntry("ActivityEventLevels")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "EventId -> EventId #key", "Level -> Level #key" })]
		public GameConfigLibrary<LevelId<EventId>, ActivityEventLevelInfo> ActivityEventLevels { get; protected set; }

		[GameConfigEntry("DailyTaskEvents")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "EventId -> EventId #key" })]
		[GameConfigSyntaxAdapter(headerPrefixReplaces: new string[]{ "# -> Schedule." }, headerReplaces: new string[]{ "#StartDate -> Schedule.Start.Date", "#StartTime -> Schedule.Start.Time" })]
		[GameConfigEntryTransform(typeof(DailyTaskEventConfigItem))]
		public GameConfigLibrary<EventId, DailyTaskEventInfo> DailyTaskEvents { get; protected set; }

		[GameConfigEntry("DailyTaskSets")]
		[GameConfigSyntaxAdapter(headerReplaces: new string[] { "DailyTaskSetId -> DailyTaskSetId #key", "Slot -> Slot #key" })]
		public GameConfigLibrary<LevelId<DailyTaskSetId>, DailyTaskSlotAlternativesInfo> DailyTaskSets { get; protected set; }
		
		[GameConfigEntry("MergeEventTemplates")]
		public GameConfigLibrary<LiveOpsEventTemplateId, LiveOpsEventTemplateConfigData<MergeEvent>> MergeEventTemplates { get; private set; }

		[GameConfigEntry("Offers")]
		[GameConfigEntryTransform(typeof(DefaultMetaOfferSourceConfigItem))]
		public GameConfigLibrary<MetaOfferId, DefaultMetaOfferInfo> Offers { get; private set; }

		[GameConfigEntry("OfferGroups")]
		[GameConfigEntryTransform(typeof(DefaultMetaOfferGroupSourceConfigItem))]
		public GameConfigLibrary<MetaOfferGroupId, DefaultMetaOfferGroupInfo> OfferGroups { get; private set; }

		public override void BuildTimeValidate(GameConfigValidationResult result)
		{
			base.BuildTimeValidate(result);

			foreach (InAppProductInfo inAppProductInfo in InAppProducts.Values) {
				string configId = $"{nameof(InAppProducts)} \"{inAppProductInfo.ProductId}\"";
				VipPassId vipPassId = inAppProductInfo.VipPassId;
				if (vipPassId != VipPassId.None) {
					if (!VipPasses.ContainsKey(vipPassId)) {
						result.Error(nameof(InAppProducts), configId, $"{configId} refers to nonexistent {nameof(VipPassId)}: \"{vipPassId}\"");
					}
					if (inAppProductInfo.VipPassDuration <= MetaDuration.Zero) {
						result.Error(nameof(InAppProducts), configId, $"{configId} is a VIP pass ({vipPassId}) but has duration {inAppProductInfo.VipPassDuration} <= 0");
					}
				}
			}

			foreach (LogbookTaskInfo logbookTaskInfo in LogbookTasks.Values) {
				string configId = $"{nameof(LogbookTasks)} \"{logbookTaskInfo.Id}\"";
				if (logbookTaskInfo.Type == LogbookTaskType.ItemCount ||
					logbookTaskInfo.Type == LogbookTaskType.ItemDiscovery ||
					logbookTaskInfo.Type == LogbookTaskType.CollectItem) {
					if (logbookTaskInfo.Item.Type == null) {
						result.Error(nameof(LogbookTasks), configId, $"{configId} must have an item configured");
					}
				} else {
					if (logbookTaskInfo.Item.Type != null) {
						result.Error(nameof(LogbookTasks), configId, $"{configId} should NOT have an item configured");
					}
				}

				if (logbookTaskInfo.Type == LogbookTaskType.ItemCount ||
					logbookTaskInfo.Type == LogbookTaskType.ItemDiscovery) {
					if (logbookTaskInfo.Item.Type == ChainTypeId.None || logbookTaskInfo.Item.Level == 0) {
						result.Error(nameof(LogbookTasks), configId, $"{configId} must be configured with a concrete item (was {logbookTaskInfo.Item})");
					}
				}

				if (logbookTaskInfo.Type == LogbookTaskType.ItemDiscovery && logbookTaskInfo.Count != 1) {
					result.Error(nameof(LogbookTasks), configId, $"{configId} (ItemDiscovery) should have 1 as count (now is {logbookTaskInfo.Count})");
				}

				if (logbookTaskInfo.Type == LogbookTaskType.UnlockIsland) {
					if (logbookTaskInfo.Count != 1) {
						result.Error(nameof(LogbookTasks), configId, 
							$"{configId} (UnlockIsland) should have 1 as count (now is {logbookTaskInfo.Count})"
						);
					}

					if (logbookTaskInfo.Island == null || logbookTaskInfo.Island == IslandTypeId.None) {
						result.Error(nameof(LogbookTasks), configId, 
							$"{configId} (UnlockIsland) should have Island configured)"
						);
					}
				}
			}

			foreach (ChainInfo chain in Chains.Values) {
				if ((chain.Width > 1 || chain.Height > 1) && chain.Movable) {
					// At the moment MergeBoard logic cannot handle movable items that are larger than 1x1.
					result.Error(nameof(Chains), chain.ConfigKey.ToString(), $"Items larger than 1x1 cannot be movable: {chain.ConfigKey})");
				}
			}

			foreach (TriggerInfo triggerInfo in Triggers.Values) {
				string configId = $"{nameof(Triggers)} \"{triggerInfo.ConfigKey}\"";
				if (triggerInfo.Dialogue != DialogueId.None && !Dialogues.ContainsKey(triggerInfo.Dialogue)) {
					result.Error(nameof(Triggers), configId, $"No such dialogue '{triggerInfo.Dialogue}' (referenced by {configId})");
				}
			}

			// See PlayerModel.MapRewardToRealType for the list of pseudo item types
			HashSet<ChainTypeId> pseudoTypes = new HashSet<ChainTypeId>();
			pseudoTypes.Add(ChainTypeId.ResourceItem);
			pseudoTypes.Add(ChainTypeId.ResourceCreator);
			pseudoTypes.Add(ChainTypeId.HeroItem);
			pseudoTypes.Add(ChainTypeId.BuildingItem);
			pseudoTypes.Add(ChainTypeId.IslandCreator);
			pseudoTypes.Add(ChainTypeId.IslandChest);

			foreach (ActivityEventLevelInfo levelInfo in ActivityEventLevels.Values) {
				string configId = $"{nameof(ActivityEventLevels)} \"{levelInfo.EventId}:{levelInfo.Level}\"";
				if (pseudoTypes.Contains(levelInfo.FreeRewardItem.Type)) {
					result.Error(nameof(ActivityEventLevels), configId, 
						$"{configId}: cannot use pseudo item type \"{levelInfo.FreeRewardItem.Type}\" as (free) reward"
					);
				}
				if (pseudoTypes.Contains(levelInfo.PremiumRewardItem.Type)) {
					result.Error(nameof(ActivityEventLevels), configId, 
						$"{configId}: cannot use pseudo item type \"{levelInfo.PremiumRewardItem.Type}\" as (premium) reward"
					);
				}
			}
		}

		protected override void OnLoaded() {
			base.OnLoaded();

			HeroResources = new OrderedSet<CurrencyTypeId>();
			foreach (HeroTaskInfo taskInfo in HeroTasks.Values) {
				foreach (ResourceInfo resource in taskInfo.Resources) {
					HeroResources.Add(resource.Type);
				}
			}

			ResourceItems = new MetaDictionary<CurrencyTypeId, ChainTypeId>();
			ChainCategories = new MetaDictionary<CategoryId, OrderedSet<ChainTypeId>>();
			foreach (ChainInfo chain in Chains.Values) {
				OrderedSet<ChainTypeId> types = ChainCategories.GetValueOrDefault(chain.Category);
				if (types == null) {
					types = new OrderedSet<ChainTypeId>();
					ChainCategories[chain.Category] = types;
				}

				types.Add(chain.Type);

				ResourceItems[chain.CollectableType] = chain.Type;
			}

			HeroItems = new MetaDictionary<ChainTypeId, HeroTypeId>();
			foreach (HeroInfo hero in Heroes.Values) {
				HeroItems[hero.ItemType] = hero.Type;
			}

			NormalHeroTasks = new List<HeroTaskInfo>();
			GoldenHeroTasks = new List<HeroTaskInfo>();
			MaxHeroTaskId = 0;
			foreach (HeroTaskInfo task in HeroTasks.Values) {
				if (task.GoldenTask) {
					GoldenHeroTasks.Add(task);
				} else {
					NormalHeroTasks.Add(task);
					MaxHeroTaskId = Math.Max(MaxHeroTaskId, task.Id);
				}
			}

			ChainMaxLevels = new MaxLevels<ChainTypeId>(Chains.Keys);
			MineMaxLevels = new MaxLevels<MineTypeId>(Mines.Keys);

			MaxIslandTaskIds = new MetaDictionary<IslanderId, int>();
			IslandTaskGroups = new MetaDictionary<IslanderId, MetaDictionary<int, List<IslandTaskInfo>>>();
			foreach (IslandTaskInfo task in IslandTasks.Values) {
				if (MaxIslandTaskIds.GetValueOrDefault(task.Islander) < task.Id) {
					MaxIslandTaskIds[task.Islander] = task.Id;
				}

                MetaDictionary<int, List<IslandTaskInfo>> taskGroups = null;
				if (IslandTaskGroups.ContainsKey(task.Islander)) {
					taskGroups = IslandTaskGroups[task.Islander];
				} else {
					taskGroups = new MetaDictionary<int, List<IslandTaskInfo>>();
					IslandTaskGroups[task.Islander] = taskGroups;
				}

				List<IslandTaskInfo> tasks = null;
				if (taskGroups.ContainsKey(task.GroupId)) {
					tasks = taskGroups[task.GroupId];
				} else {
					tasks = new List<IslandTaskInfo>();
					taskGroups[task.GroupId] = tasks;
				}

				tasks.Add(task);
			}

			LogbookTasksByChapter = new MetaDictionary<LogbookChapterId, List<LogbookTaskInfo>>();
			foreach (LogbookChapterId id in LogbookChapters.Keys) {
				LogbookTasksByChapter[id] = new List<LogbookTaskInfo>();
			}

			foreach (LogbookTaskInfo task in LogbookTasks.Values) {
				List<LogbookTaskInfo> tasks = LogbookTasksByChapter.GetValueOrDefault(task.Chapter);
				if (tasks == null) {
					throw new Exception($"Logbook task {task.Id} refers to a nonexistent chapter '{task.Chapter}'");
				}
				tasks.Add(task);
			}

			IslandLockAreas = new MetaDictionary<IslandTypeId, List<LockAreaInfo>>();
			foreach (IslandTypeId island in Islands.Keys) {
				IslandLockAreas[island] = new List<LockAreaInfo>();
			}
			foreach (LockAreaInfo lockArea in LockAreas.Values) {
				IslandLockAreas[lockArea.IslandId].Add(lockArea);
			}

			IslandBuildingFragments = new MetaDictionary<IslandTypeId, List<ChainTypeId>>();
			// To avoid unnecessary ContainsKey checking.
			foreach (IslandTypeId island in Islands.Keys) {
				IslandBuildingFragments[island] = new List<ChainTypeId>();
			}
			foreach (BuildingFragmentInfo fragment in BuildingFragments.Values) {
				IslandBuildingFragments[fragment.Island].Add(fragment.Type);
			}

			ActivityEventLevelsByEvent = new MetaDictionary<EventId, List<ActivityEventLevelInfo>>();
			foreach (ActivityEventLevelInfo level in ActivityEventLevels.Values) {
				List<ActivityEventLevelInfo> eventLevels = ActivityEventLevelsByEvent.GetValueOrDefault(level.EventId);
				if (eventLevels == null) {
					eventLevels = new List<ActivityEventLevelInfo>();
					ActivityEventLevelsByEvent[level.EventId] = eventLevels;
				}
				eventLevels.Add(level);
			}

			DailyTasksById = new MetaDictionary<DailyTaskSetId, List<DailyTaskSlotAlternativesInfo>>();
			foreach (DailyTaskSlotAlternativesInfo info in DailyTaskSets.Values) {
				List<DailyTaskSlotAlternativesInfo> dailyTaskSetInfos = DailyTasksById.GetValueOrDefault(info.DailyTaskSetId);
				if (dailyTaskSetInfos == null) {
					dailyTaskSetInfos = new List<DailyTaskSlotAlternativesInfo>();
					DailyTasksById[info.DailyTaskSetId] = dailyTaskSetInfos;
				}
				dailyTaskSetInfos.Add(info);
			}
		}

		public MaxLevels<ChainTypeId> ChainMaxLevels { get; private set; }
		public MaxLevels<MineTypeId> MineMaxLevels { get; private set; }
		public MetaDictionary<CategoryId, OrderedSet<ChainTypeId>> ChainCategories { get; private set; }

		public OrderedSet<CurrencyTypeId> HeroResources { get; private set; }
		public MetaDictionary<CurrencyTypeId, ChainTypeId> ResourceItems { get; private set; }
		public MetaDictionary<ChainTypeId, HeroTypeId> HeroItems { get; private set; }

		public int MaxHeroTaskId { get; private set; }
		public List<HeroTaskInfo> NormalHeroTasks { get; private set; }
		public List<HeroTaskInfo> GoldenHeroTasks { get; private set; }

		public MetaDictionary<IslanderId, int> MaxIslandTaskIds { get; private set; }
		public MetaDictionary<IslanderId, MetaDictionary<int, List<IslandTaskInfo>>> IslandTaskGroups { get; private set; }
		public MetaDictionary<LogbookChapterId, List<LogbookTaskInfo>> LogbookTasksByChapter { get; private set; }
		public MetaDictionary<IslandTypeId, List<LockAreaInfo>> IslandLockAreas { get; private set; }

		public MetaDictionary<IslandTypeId, List<ChainTypeId>> IslandBuildingFragments { get; private set; }

		public MetaDictionary<EventId, List<ActivityEventLevelInfo>> ActivityEventLevelsByEvent { get; private set; }

		public List<IslandTaskInfo> GetIslanderTasks(IslanderId islander, int groupId) {
			if (IslandTaskGroups.ContainsKey(islander)) {
                MetaDictionary<int, List<IslandTaskInfo>> group = IslandTaskGroups[islander];
				if (group.ContainsKey(groupId)) {
					return group[groupId];
				}

				return new List<IslandTaskInfo>();
			}

			return new List<IslandTaskInfo>();
		}

		public MetaDictionary<DailyTaskSetId, List<DailyTaskSlotAlternativesInfo>> DailyTasksById { get; private set; }
	}

	public class ServerGameConfig : ServerGameConfigBase { 
		[GameConfigEntry(PlayerExperimentsEntryName)]
		public GameConfigLibrary<PlayerExperimentId, PlayerExperimentInfo> PlayerExperiments { get; private set; }
	}

	public class GameConfigBuildIntegration : Metaplay.Core.Config.GameConfigBuildIntegration {
		public override IEnumerable<GameConfigBuildSource> GetAvailableGameConfigBuildSources(string sourcePropertyInBuildParams) {
			if (sourcePropertyInBuildParams == nameof(DefaultGameConfigBuildParameters.DefaultSource)) {
				return new GameConfigBuildSource[] {
					new GoogleSheetBuildSource("Demo", "1X5v-ZsD98JGoK7s5kopOzVKm9wsQmo9pfwUnlWOE74U", "https://script.google.com/macros/s/AKfycbyeRscNTYTgrxYb-H612cgMu-YB2LW2awQURZeNKIZ7Vvlbbsu9inDiYjv43-VqMabEDg/exec"),
					new GoogleSheetBuildSource("Develop", "1n7xqND50q3iad6uYKqTI3VsU9g6m3dSSH3JsVvAePFg", "https://script.google.com/macros/s/AKfycbzm3XCzX2p51juDnyT1RSeazRsoK0Tnnx2c3dGu2UfgWlm0jYVuligRamGrP6d9tdhA/exec"),
				};
			}

			return base.GetAvailableGameConfigBuildSources(sourcePropertyInBuildParams);
		}

		public override LocalizationsBuild MakeLocalizationsBuild(IGameConfigSourceFetcherConfig fetcherConfig) {
			IGameConfigSourceFetcherProvider sourceFetcherProvider = MakeSourceFetcherProvider(fetcherConfig);
			return new LocalizationBuild();
		}
	}

	public class GameConfigBuild : GameConfigBuildTemplate<SharedGameConfig, ServerGameConfig> {
		Random rand = 
#if NETCOREAPP
			Random.Shared;
#else
new System.Random();
#endif
		public float NextSingle()
		{
#if NETCOREAPP
			return rand.NextSingle();
#else
			return (float)rand.NextDouble();
#endif
		}
		

		public OrcaGameConfigBuildParameters BuildParameters => BuildParametersBase as OrcaGameConfigBuildParameters;

		protected override ConfigEntryBuilder? GetEntryBuilder(Type configType, string entryName) {
			if (entryName == "Chains") {
				return CustomEntryBuildSingleSource<SpreadsheetContent>(
					entryName,
					(builder, type) => {
						SpreadsheetContent content = ModifySpreadsheet(type, builder, BuildParameters.GenerateDiffs, BuildParameters.GenerateDuplicateBuildErrors, BuildParameters.GenerateIncompatibleBuildErrors, BuildParameters.GenerateWarnings);
						builder.BuildGameConfigEntry(entryName, content);
					}
				);
			}

			return base.GetEntryBuilder(configType, entryName);
		}

		private SpreadsheetContent ModifySpreadsheet(
			SpreadsheetContent content,
			IGameConfigBuilder builder,
			bool modifyData,
			bool duplicateData,
			bool breakData,
			bool generateWarnings
		) {
			var header = content.Cells[0];

			var typeIndex = header.FindIndex(y => y.Value == "Type");
			var categoryIndex = header.FindIndex(y => y.Value == "Category");
			var valueIndex = header.FindIndex(y => y.Value == "CollectableValue");
			var levelIndex = header.FindIndex(y => y.Value == "Level");
			var groupBy = content.Cells.Skip(1).GroupBy(x=> x[typeIndex].Value);

			int addedRowCount = 0;
			List<List<SpreadsheetCell>> rows = new List<List<SpreadsheetCell>>();
			rows.Add(header);
			
			foreach (var grouping in groupBy) {
				if (grouping.First()[categoryIndex].Value == "Resources") {
					if (modifyData) {
						if (NextSingle() < .5f) {
							rows.AddRange(grouping);
							var last = grouping.Last();

							if (string.IsNullOrWhiteSpace(last[valueIndex].Value))
								continue;

							var spreadsheetCells = new List<SpreadsheetCell>(last);
							var valueCell = spreadsheetCells[valueIndex];
							spreadsheetCells[valueIndex] = new SpreadsheetCell((int.Parse(valueCell.Value) * 2).ToString(), valueCell.Row, valueCell.Column);
							var levelCell = spreadsheetCells[levelIndex];
							spreadsheetCells[levelIndex] = new SpreadsheetCell((int.Parse(levelCell.Value) + 1).ToString(), levelCell.Row, levelCell.Column);

							for (var i = 0; i < spreadsheetCells.Count; i++) {
								spreadsheetCells[i] = new SpreadsheetCell(spreadsheetCells[i].Value, content.Cells.Count + addedRowCount, spreadsheetCells[i].Column);
							}

							addedRowCount++;

							rows.Add(spreadsheetCells);
						} else {
							var count = grouping.Count();
							var randVal = rand.Next(0, count);
							int index = 0;
							foreach (var row in grouping) {
								if (index == randVal && !string.IsNullOrWhiteSpace(row[valueIndex].Value) && NextSingle() < .1f) {
									var spreadsheetCells = new List<SpreadsheetCell>(row);
									var valueCell = spreadsheetCells[valueIndex];
									spreadsheetCells[valueIndex] = new SpreadsheetCell((int.Parse(valueCell.Value) * 2).ToString(), valueCell.Row, valueCell.Column);
									rows.Add(spreadsheetCells);
								} else {
									rows.Add(row);
								}

								index++;
							}
						}
					} else {
						rows.AddRange(grouping);
					}
				} else if (duplicateData) {
					var count = grouping.Count();
					var randVal = rand.Next(0, count);
					int index = 0;
					foreach (var row in grouping) {
						if (index == randVal) {
							if (NextSingle() < .1f) {
								rows.Add(row);
								rows.Add(row);
							} else {
								rows.Add(row);
							}
						} else {
							rows.Add(row);
						}

						index++;
					}
				} else if (breakData) {
					
					var count = grouping.Count();
					var randVal = rand.Next(0, count);
					int index = 0;
					foreach (var row in grouping) {
						if (index == randVal) {
							if (NextSingle() < .1f) {
								var spreadsheetCells = new List<SpreadsheetCell>(row);
								var valueCell = spreadsheetCells[valueIndex];
								spreadsheetCells[valueIndex] = new SpreadsheetCell(((char)('A' + rand.Next(26))).ToString(), valueCell.Row, valueCell.Column);
								rows.Add(spreadsheetCells);
							} else {
								rows.Add(row);
							}
						} else {
							rows.Add(row);
						}

						index++;
					}
				} else {
					rows.AddRange(grouping);
				}
			}

			if (generateWarnings) {
				foreach (var cells in rows) {
					if (NextSingle() < 0.05f) {
						var cellIndex = rand.Next(cells.Count);
						var cell = cells[cellIndex];
						var columnName = header[cellIndex].Value;

						var warningTypes = new[] {
							$"Unusual data pattern detected in {columnName}",
							$"Consider reviewing {columnName} value: '{cell.Value}'",
							$"Data validation flagged {columnName} for manual review",
							$"{columnName} value may need balancing adjustment",
							$"{columnName} setting deviates from established patterns",
							$"Automated analysis suggests {columnName} requires attention",
							$"Review recommended: {columnName} value seems inconsistent",
						};

						string warningMessage = warningTypes[rand.Next(warningTypes.Length)];

						builder.BuildLog.WithSource(content.SourceInfo).WithLocation(
							new GameConfigSpreadsheetLocation(
								content.SourceInfo,
								new GameConfigSpreadsheetLocation.CellRange(cell.Row, cell.Row + 1),
								new GameConfigSpreadsheetLocation.CellRange(cell.Column, cell.Column + 1)
							)
						).Warning(warningMessage);
					}
				}
			}

			return new SpreadsheetContent(content.Name, rows.OrderBy(x=>x[0].Row).ToList(), content.SourceInfo);
		}
	}

	[MetaSerializableDerived(2)]
	public class OrcaGameConfigBuildParameters : GameConfigBuildParameters {
		public override bool IsIncremental => false;

		[MetaMember(1)]
		public bool GenerateDuplicateBuildErrors;
		[MetaMember(2)]
		public bool GenerateIncompatibleBuildErrors;
		[MetaMember(3)]
		public bool GenerateWarnings;
		[MetaMember(4)]
		public bool GenerateDiffs;
	}
}
