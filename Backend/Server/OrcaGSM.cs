using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Logic;
using Game.Logic.LiveOpsEvents;
using Metaplay.Cloud.Entity;
using Metaplay.Cloud.Persistence;
using Metaplay.Cloud.RuntimeOptions;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.LiveOpsEvent;
using Metaplay.Core.Player;
using Metaplay.Core.Schedule;
using Metaplay.Server;
using Metaplay.Server.Database;
using Metaplay.Server.GameConfig;
using Metaplay.Server.LiveOpsEvent;
using Metaplay.Server.LiveOpsTimeline;
using Metaplay.Server.LiveOpsTimeline.Timeline;
using GameConfigBuildIntegration = Metaplay.Core.Config.GameConfigBuildIntegration;

namespace Game.Server;

/// <summary>
/// This class seeds the default state for demo environments, i.e. create events, game configs, localizations that contain more interesting data for presentation purposes
/// </summary>
public class OrcaGSM {

	public class OrcaGlobalStateManager : GlobalStateManagerBase<DefaultGlobalState>
	{
		public OrcaGlobalStateManager(EntityId entityId) : base(entityId)
		{
		}

		protected override Task<DefaultGlobalState> InitializeNew()
		{
			// Create new state
			DefaultGlobalState state = new DefaultGlobalState();
			return Task.FromResult(state);
		}

		private readonly string[] hexColors = [
			"#c34a2f",
			"#ebbf34",
			"#3f6730",
			"#4b99e3",
			"#4b4cb3",
			"#7d63c9",
			"#8702a8",
			"#616161"
		];

		private string GetRandomColor() {
			return hexColors[new Random().Next(0, hexColors.Length)];
		}

		private static readonly List<(int day, int month, int durationInDays, string name, string description)> HolidayEvents = [
			new(2, 11, 1, "All Saints' Day", ""),
			new(10, 11, 1, "Father's Day", ""),
			new(6, 12, 1, "Independence Day", ""),
			new(24, 12, 3, "Christmas", "Winter Wonderland"),
			new(31, 12, 2, "New Year's", "Midnight Mingle"),
			new(6, 1, 1, "Epiphany", ""),
			new(14, 2, 1, "Valentine's Day", "Cupid's Crafting Corner"),
			new(18, 4, 4, "Easter", "Bunny's Retreat"),
			new(1, 5, 1, "May Day", ""),
			new(11, 5, 1, "Mother's Day", ""),
			new(29, 5, 1, "Ascension Day", ""),
			new(8, 6, 1, "Whit Sunday", ""),
			new(20, 6, 2, "Midsummer", "Sunset Soiree"),
			new(1, 4, 1, "April Fools", "Trickster's Tale"),
			new(31, 10, 1, "Halloween", "Haunted Harvest"),
			new(28, 11, 1, "Thanksgiving", "Gratitude Gathering")
		];

		private static readonly List<(string name, string description, DayOfWeek dayOfWeek, int durationInDays)> WeeklyEvents = [
			new("Midweek Chill", "", DayOfWeek.Wednesday, 1),
			new("Workday Wonders", "", DayOfWeek.Tuesday, 3),
			new("Midweek Mirage", "", DayOfWeek.Wednesday, 1),
			new("Homey Hearth Week", "", DayOfWeek.Monday, 5),
			new("Cozy Cottage Week", "", DayOfWeek.Monday, 5),
			new("A Week in Wonderland", "", DayOfWeek.Monday, 5),
			new("Whispers of the Woods", "", DayOfWeek.Friday, 1),
			new("Restful Rewind", "", DayOfWeek.Thursday, 2),
			new("Pillow Fort Paradise", "", DayOfWeek.Tuesday, 1),
			new("Patchwork Paradise", "", DayOfWeek.Friday, 1),
		];

		private static readonly List<(string name, string description)> WeekendEvents = [
			new("Double XP Weekend", "Earn twice the XP all weekend long!"),
			new("Guild Madness", "Compete with other guilds, who can score the most posts collectively"),
			new("Weekend Treasure Hunt", "Find hidden treasures throughout the game"),
			new("Weekend Garden Bloom", "Watch your garden flourish with special weekend bonuses"),
			new("Lucky Weekend Spin", "Extra lucky spins all weekend")
		];

		private static readonly List<(string name, string description, int month)> BattlePassEvents = [
			new("Winter's Embrace", "A battle pass inspired by the soothing moments of winter—think snuggling up by the fire with soft blankets, hot cocoa, and warm woolen scarves.", 1),
			new("Heartfelt Haven", "A cozy, heartwarming battle pass focused on connection, affection, and personal comfort.", 2),
			new("Springtime Serenity", "Embracing the peaceful rebirth of spring, this battle pass brings in light pastel colors, soft floral designs, and calm natural motifs.", 3),
			new("Whispers of the Forest", "This battle pass takes players into the heart of a quiet, serene forest where every leaf rustles gently in the breeze.", 4),
			new("Hearth & Home", "A heartwarming theme celebrating the comforts of home and hearth.", 5),
			new("Cozy Cottage", "This battle pass brings a sense of quaint rural living, with cozy cottagecore aesthetics.", 6),
			new("Summer Breeze", "A summer battle pass centered around campfires, storytelling, and gathering with friends.", 7),
			new("Fireside Tales", "Soft, breezy, and calm, this battle pass reflects the easygoing vibes of late summer.", 8),
			new("Autumn's Embrace", "Cozy up to fall with rich golden, amber, and burgundy tones, featuring soft woolen scarves, pumpkin spice vibes, and comfortable fall jackets.", 9),
			new("Harvest Moon", "Embrace the gentle, harvest-themed coziness of October with soft, moonlit imagery, and harvest-themed skins.", 10),
			new("Golden Hearth", "A cozy theme that centers around the golden tones of autumn, the warmth of a crackling hearth, and the comfort of family gatherings.", 11),
			new("Twilight Snowfall", "A serene, winter-themed battle pass inspired by the gentle beauty of snowfall at twilight.", 12),
		];

		protected override void PreStart() {
			base.PreStart();

			Task.Run(CreateLiveOpsEvents);
			Task.Run(CreateGameConfigs);
			Task.Run(CreateLocalizations);
			Task.Run(RolloutExperiments);
		}

		private async Task RolloutExperiments() {
			try {
				await ChangeExperimentPhase("EnergyCosts", PlayerExperimentPhase.Ongoing);
				await ChangeExperimentPhase("EnergyRegen", PlayerExperimentPhase.Testing);
				await ChangeExperimentPhase("FasterLeveling", PlayerExperimentPhase.Ongoing);
				await ChangeExperimentPhase("FasterLeveling", PlayerExperimentPhase.Paused);
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}

		private async Task ChangeExperimentPhase(string experimentId, PlayerExperimentPhase phase) {
			try {
				PlayerExperimentId id = PlayerExperimentId.FromString(experimentId);
				GlobalStateSetExperimentPhaseResponse response = await EntityAskAsync<GlobalStateSetExperimentPhaseResponse>(GlobalStateManager.EntityId,
					new GlobalStateSetExperimentPhaseRequest(
						playerExperimentId:     id,
						phase:                  phase,
						force:                  true));

				if (response.ErrorStringOrNull == null)
				{
					_log.Info("Experiment phase modified successfully.");
				}
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}

		private async Task CreateGameConfigs() {
			try {
				await Task.Delay(TimeSpan.FromSeconds(30));

				_log.Info("Checking Game configs");
				MetaDatabase db = MetaDatabase.Get(QueryPriority.Normal);
				IEnumerable<PersistedStaticGameConfig> configs = await db.QueryAllStaticGameConfigs(true);

				if (configs.Count() < 3) {
					_log.Info("Creating Game configs");
					string envName;
					if (RuntimeOptionsBase.IsDemoEnvironment || RuntimeOptionsBase.IsSalesEnvironment) {
						envName = "Demo";
					} else {
						envName = "Develop";
					}

					var source = IntegrationRegistry.Get<GameConfigBuildIntegration>()
						.GetAvailableGameConfigBuildSources(nameof(GameConfigBuildParameters.DefaultSource))
						.FirstOrDefault(x => x.DisplayName == envName);

					StartGameConfigBuild("GameConfig with Dupe Errors", "", new OrcaGameConfigBuildParameters { DefaultSource = source, GenerateDuplicateBuildErrors = true });
					StartGameConfigBuild("GameConfig with Incompatible Errors", "", new OrcaGameConfigBuildParameters { DefaultSource = source, GenerateIncompatibleBuildErrors = true });
					StartGameConfigBuild("GameConfig with Warnings", "", new OrcaGameConfigBuildParameters { DefaultSource = source, GenerateWarnings = true });
					StartGameConfigBuild("GameConfig with Diffs", "", new OrcaGameConfigBuildParameters { DefaultSource = source, GenerateDiffs = true });
				}
			} catch (Exception e) {
				_log.LogEvent(LogLevel.Error, e, "Exception occured");
			}

		}

		private async Task StartGameConfigBuild(string name, string description, OrcaGameConfigBuildParameters parameters) {
			try {
				MetaGuid taskId = MetaGuid.New();

				MetaGuid configId = (await EntityAskAsync<CreateOrUpdateGameDataResponse>(
					GlobalStateManager.EntityId,
					new CreateOrUpdateGameConfigRequest() {
						Source = "Generated",
						Name = name,
						Description = description,
						IsArchived = false,
						TaskId = taskId
					}
				)).Id;

				// Start the build task
				BuildStaticGameConfigTask buildTask = new BuildStaticGameConfigTask(
					configId,
					MetaGuid.None,
					parameters
				);
				_ = await EntityAskAsync<StartBackgroundTaskResponse>(
					BackgroundTaskActor.EntityId,
					new StartBackgroundTaskRequest(taskId, buildTask)
				);
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}
		private async Task CreateLocalizations() {
			try {
				await Task.Delay(TimeSpan.FromSeconds(30));

				_log.Info("Checking Game configs");
				MetaDatabase db = MetaDatabase.Get(QueryPriority.Normal);
				IEnumerable<PersistedStaticGameConfig> configs = await db.QueryAllStaticGameConfigs(true);

				if (configs.Count() < 3) {
					_log.Info("Creating Game configs");
					string envName;
					if (RuntimeOptionsBase.IsDemoEnvironment || RuntimeOptionsBase.IsSalesEnvironment) {
						envName = "Demo";
					} else {
						envName = "Develop";
					}

					var source = IntegrationRegistry.Get<GameConfigBuildIntegration>()
						.GetAvailableGameConfigBuildSources(nameof(GameConfigBuildParameters.DefaultSource))
						.FirstOrDefault(x => x.DisplayName == envName);

					StartLocalizationBuild("Localization with Diffs", "", new OrcaLocalizationBuildParameters { DefaultSource = source, GenerateDiffs = true });
				}
			} catch (Exception e) {
				_log.LogEvent(LogLevel.Error, e, "Exception occured");
			}

		}

		private async Task StartLocalizationBuild(string name, string description, LocalizationsBuildParameters parameters) {
			try {
				MetaGuid taskId = MetaGuid.New();

				MetaGuid configId = (await EntityAskAsync<CreateOrUpdateGameDataResponse>(
					GlobalStateManager.EntityId,
					new CreateOrUpdateLocalizationsRequest() {
						Source = "Generated",
						Name = name,
						Description = description,
						IsArchived = false,
						TaskId = taskId
					}
				)).Id;

				// Start the build task
				BuildLocalizationsTask buildTask = new BuildLocalizationsTask(
					configId,
					parameters
				);
				_ = await EntityAskAsync<StartBackgroundTaskResponse>(
					BackgroundTaskActor.EntityId,
					new StartBackgroundTaskRequest(taskId, buildTask)
				);
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}

		[MessageHandler]
		public void HandleSetLiveOpsEventsMessage(SetLiveOpsEventsMessage message)
		{

		}

		private async Task CreateLiveOpsGroups() {
			try {
				(EntitySubscription subscription, LiveOpsTimelineManagerSubscribeResponse timelineResponse) = await SubscribeToAsync<LiveOpsTimelineManagerSubscribeResponse>(LiveOpsTimelineManager.EntityId, EntityTopic.Member, new LiveOpsTimelineManagerSubscribeRequest());
				LiveOpsTimelineManagerState state = timelineResponse.State.Deserialize(resolver: null, logicVersion: null);
				var timelineState = state.Timeline;
				await UnsubscribeFromAsync(subscription);

				var groupId = timelineState.Nodes.FirstOrDefault(x=> x.Value.NodeType == NodeType.Group).Key;

				if (timelineState.Nodes.All(x => x.Value.DisplayName != "Weekly")) {
					await EntityAskAsync(
						LiveOpsTimelineManager.EntityId,
						new InvokeLiveOpsTimelineCommandRequest(
							new CreateNewItemCommand(
								NodeType.Row,
								new MetaDictionary<ItemMetadataField, string>() {
									{ ItemMetadataField.DisplayName, "Weekly" }
								},
								new ItemId.PersistentNode(groupId),
								parentVersion: 0
							)
						)
					);
				}
				if (timelineState.Nodes.All(x => x.Value.DisplayName != "Weekend")) {
					await EntityAskAsync(
						LiveOpsTimelineManager.EntityId,
						new InvokeLiveOpsTimelineCommandRequest(
							new CreateNewItemCommand(
								NodeType.Row,
								new MetaDictionary<ItemMetadataField, string>() {
									{ ItemMetadataField.DisplayName, "Weekend" }
								},
								new ItemId.PersistentNode(groupId),
								parentVersion: 0
							)
						)
					);
				}
				if (timelineState.Nodes.All(x => x.Value.DisplayName != "Holidays")) {
					await EntityAskAsync<InvokeLiveOpsTimelineCommandResponse>(
						LiveOpsTimelineManager.EntityId,
						new InvokeLiveOpsTimelineCommandRequest(
							new CreateNewItemCommand(
								NodeType.Row,
								new MetaDictionary<ItemMetadataField, string>() {
									{ ItemMetadataField.DisplayName, "Holidays" }
								},
								new ItemId.PersistentNode(groupId),
								parentVersion: 0
							)
						)
					);
				}
				if (timelineState.Nodes.All(x => x.Value.DisplayName != "Battle Pass")) {
					await EntityAskAsync<InvokeLiveOpsTimelineCommandResponse>(
						LiveOpsTimelineManager.EntityId,
						new InvokeLiveOpsTimelineCommandRequest(
							new CreateNewItemCommand(
								NodeType.Row,
								new MetaDictionary<ItemMetadataField, string>() {
									{ ItemMetadataField.DisplayName, "Battle Pass" }
								},
								new ItemId.PersistentNode(groupId),
								parentVersion: 0
							)
						)
					);
				}
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}

		private async Task CreateLiveOpsEvents() {
			try {
				await CreateLiveOpsGroups();

				GetLiveOpsEventsResponse events = await EntityAskAsync<GetLiveOpsEventsResponse>(
					LiveOpsTimelineManager.EntityId,
					new GetLiveOpsEventsRequest()
				);

				var lastEventAt = events.Occurrences.MaxBy(x => x?.UtcScheduleOccasionMaybe?.GetEnabledEndTime().MillisecondsSinceEpoch)
					?.UtcScheduleOccasionMaybe?.GetEnabledEndTime() ??
					MetaTime.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc));

				for (int i = 0; i < 365; i++) {
					var date = (lastEventAt + MetaDuration.FromDays(i)).ToDateTime();
					// Only create events for the next year
					if (date > DateTime.Now.AddDays(365))
						break;

					var startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

					// Holidays
					foreach (var (day, month, durationInDays, name, description) in HolidayEvents) {
						if (day == date.Day && month == date.Month) {
							LiveOpsEventContent content = new CurrencyMultiplierEvent() {
								Multiplier = 2,
								Type = CurrencyTypeId.Gems
							};
							await EntityAskAsync<CreateLiveOpsEventResponse>(
								LiveOpsTimelineManager.EntityId,
								new CreateLiveOpsEventRequest(
									validateOnly: false,
									new LiveOpsEventSettings(
										new MetaRecurringCalendarSchedule(
											MetaScheduleTimeMode.Utc,
											MetaCalendarDateTime.FromDateTime(startDate),
											new MetaCalendarPeriod(0, 0, durationInDays, 0, 0, 0),
											new MetaCalendarPeriod(0, 0, 0, 2, 0, 0),
											new MetaCalendarPeriod(0, 0, durationInDays, 0, 0, 0),
											new MetaCalendarPeriod(),
											null,
											null
										),
										new LiveOpsEventParams(
											name,
											description,
											GetRandomColor(),
											new List<EntityId>(),
											null,
											null,
											content
										)
									)
								)
							);
						}
					}

					// Weekend Events
					if (date.DayOfWeek == DayOfWeek.Saturday) {
						LiveOpsEventContent content = new CurrencyMultiplierEvent() {
							Multiplier = 2,
							Type = CurrencyTypeId.Xp
						};
						int index = Random.Shared.Next(0, WeekendEvents.Count);
						var (name, description) = WeekendEvents[index];
						await EntityAskAsync<CreateLiveOpsEventResponse>(
							LiveOpsTimelineManager.EntityId,
							new CreateLiveOpsEventRequest(
								validateOnly: false,
								new LiveOpsEventSettings(
									new MetaRecurringCalendarSchedule(
										MetaScheduleTimeMode.Utc,
										MetaCalendarDateTime.FromDateTime(startDate),
										new MetaCalendarPeriod(0, 0, 2, 0, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 0, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 0, 0, 0),
										new MetaCalendarPeriod(),
										null,
										null
									),
									new LiveOpsEventParams(
										name,
										description,
										GetRandomColor(),
										new List<EntityId>(),
										null,
										null,
										content
									)
								)
							)
						);
					}

					// Weekly Events
					if (date.DayOfWeek == DayOfWeek.Sunday) {
						int index = Random.Shared.Next(0, WeeklyEvents.Count);
						var (name, description, dayOfWeek, durationInDays) = WeeklyEvents[index];
						// Calculate the date for the specified day of week in the current week
						int daysUntilEvent = ((int)dayOfWeek - (int)date.DayOfWeek + 7) % 7;
						var startTime = date.AddDays(daysUntilEvent).Date;
						LiveOpsEventContent content = new CurrencyMultiplierEvent() {
							Multiplier = 2,
							Type = CurrencyTypeId.Xp
						};
						await EntityAskAsync<CreateLiveOpsEventResponse>(
							LiveOpsTimelineManager.EntityId,
							new CreateLiveOpsEventRequest(
								validateOnly: false,
								new LiveOpsEventSettings(
									new MetaRecurringCalendarSchedule(
										MetaScheduleTimeMode.Utc,
										MetaCalendarDateTime.FromDateTime(startTime),
										new MetaCalendarPeriod(0, 0, durationInDays, 0, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 0, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 0, 0, 0),
										new MetaCalendarPeriod(),
										null,
										null
									),
									new LiveOpsEventParams(
										name,
										description,
										hexColors[(int)dayOfWeek],
										new List<EntityId>(),
										null,
										null,
										content
									)
								)
							)
						);
					}

					// Battle Passes
					if (date.Day == 1) {
						// Create a battle pass event that lasts for the entire month
						var month = date.Month;
						var daysInMonth = DateTime.DaysInMonth(date.Year, month);
						var (name, description, _) = BattlePassEvents.FirstOrDefault(x => x.month == month);
						if (name != null) {
							LiveOpsEventContent content = new CurrencyMultiplierEvent() {
								Multiplier = 1.5f,
								Type = CurrencyTypeId.Xp
							};
							await EntityAskAsync<CreateLiveOpsEventResponse>(
								LiveOpsTimelineManager.EntityId,
								new CreateLiveOpsEventRequest(
									validateOnly: false,
									new LiveOpsEventSettings(
										new MetaRecurringCalendarSchedule(
											MetaScheduleTimeMode.Utc,
											MetaCalendarDateTime.FromDateTime(startDate),
											new MetaCalendarPeriod(0, 0, daysInMonth, 0, 0, 0),
											new MetaCalendarPeriod(0, 0, 0, 0, 0, 0),
											new MetaCalendarPeriod(0, 0, 0, 0, 0, 0),
											new MetaCalendarPeriod(),
											null,
											null
										),
										new LiveOpsEventParams(
											name,
											description,
											hexColors[month % (hexColors.Length - 1)],
											new List<EntityId>(),
											null,
											null,
											content
										)
									)
								)
							);
						}
					}

					// Flash Sales
					if (date.DayOfWeek == DayOfWeek.Monday) {
						// Pick a random day in the week
						int randomDayOffset = Random.Shared.Next(0, 7);
						DateTime randomDay = startDate.AddDays(randomDayOffset);

						// Pick a random hour
						int randomHour = Random.Shared.Next(0, 24);
						DateTime startTime = new DateTime(randomDay.Year, randomDay.Month, randomDay.Day, randomHour, 0, 0, DateTimeKind.Utc);

						LiveOpsEventContent content = new CurrencyMultiplierEvent() {
							Multiplier = 3,
							Type = CurrencyTypeId.Gems
						};

						await EntityAskAsync<CreateLiveOpsEventResponse>(
							LiveOpsTimelineManager.EntityId,
							new CreateLiveOpsEventRequest(
								validateOnly: false,
								new LiveOpsEventSettings(
									new MetaRecurringCalendarSchedule(
										MetaScheduleTimeMode.Utc,
										MetaCalendarDateTime.FromDateTime(startTime),
										new MetaCalendarPeriod(0, 0, 0, 1, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 0, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 1, 0, 0),
										new MetaCalendarPeriod(),
										null,
										null
									),
									new LiveOpsEventParams(
										"Flash Sale!",
										"Limited time offer - Triple gems!",
										"#ff0000",
										new List<EntityId>(),
										null,
										null,
										content
									)
								)
							)
						);
					}
				}

				await MoveLiveOpsEvents();
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}

		private async Task MoveLiveOpsEvents()
		{
			try {
				(EntitySubscription subscription, LiveOpsTimelineManagerSubscribeResponse timelineResponse) =
					await SubscribeToAsync<LiveOpsTimelineManagerSubscribeResponse>(
						LiveOpsTimelineManager.EntityId,
						EntityTopic.Member,
						new LiveOpsTimelineManagerSubscribeRequest()
					);
				LiveOpsTimelineManagerState state = timelineResponse.State.Deserialize(
					resolver: null,
					logicVersion: null
				);

				var timelineState = state.Timeline;
				await UnsubscribeFromAsync(subscription);

				GetLiveOpsEventsResponse events = await EntityAskAsync<GetLiveOpsEventsResponse>(
					LiveOpsTimelineManager.EntityId,
					new GetLiveOpsEventsRequest()
				);

				var weeklyNodeId = timelineState.Nodes.FirstOrDefault(x => x.Value.DisplayName == "Weekly").Key;
				var weekendNodeId = timelineState.Nodes.FirstOrDefault(x => x.Value.DisplayName == "Weekend").Key;
				var holidaysNodeId = timelineState.Nodes.FirstOrDefault(x => x.Value.DisplayName == "Holidays").Key;
				var battlePassNodeId = timelineState.Nodes.FirstOrDefault(x => x.Value.DisplayName == "Battle Pass").Key;

				// Move existing events to the appropriate row
				foreach (var occurrence in events.Occurrences) {
					try {
						var holidayEvent = HolidayEvents.FirstOrDefault(e =>
							occurrence.UtcScheduleOccasionMaybe != null &&
							occurrence.EventParams.DisplayName == e.name &&
							occurrence.EventParams.Description == e.description
						);

						var weekendEvent = WeekendEvents.FirstOrDefault(e =>
							occurrence.UtcScheduleOccasionMaybe != null &&
							occurrence.EventParams.DisplayName == e.name &&
							occurrence.EventParams.Description == e.description
						);

						var weeklyEvent = WeeklyEvents.FirstOrDefault(e =>
							occurrence.UtcScheduleOccasionMaybe != null &&
							occurrence.EventParams.DisplayName == e.name &&
							occurrence.EventParams.Description == e.description
						);

						var battlePassEvent = BattlePassEvents.FirstOrDefault(e =>
							occurrence.UtcScheduleOccasionMaybe != null &&
							occurrence.EventParams.DisplayName == e.name &&
							occurrence.EventParams.Description == e.description
						);

						if (holidayEvent != default) {
							// Move the event to the Holidays row
							await EntityAskAsync(
								LiveOpsTimelineManager.EntityId,
								new InvokeLiveOpsTimelineCommandRequest(
									new MoveItemsCommand(
										[
											new MoveItemsCommand.ItemMove(
												new ItemId.Element(
													new LiveOpsEventTimelineDataSource.LiveOpsEventId(
														occurrence.OccurrenceId
													)
												),
												currentVersion: 0,
												parentVersion: 0
											)
										],
										new MoveItemsCommand.NewParentInfo(
											new ItemId.PersistentNode(holidaysNodeId),
											currentVersion: 0,
											insertIndex: 0
										)
									)
								)
							);
						}
						if (weekendEvent != default) {
							// Move the event to the Weekend row
							await EntityAskAsync(
								LiveOpsTimelineManager.EntityId,
								new InvokeLiveOpsTimelineCommandRequest(
									new MoveItemsCommand(
										[
											new MoveItemsCommand.ItemMove(
												new ItemId.Element(
													new LiveOpsEventTimelineDataSource.LiveOpsEventId(
														occurrence.OccurrenceId
													)
												),
												currentVersion: 0,
												parentVersion: 0
											)
										],
										new MoveItemsCommand.NewParentInfo(
											new ItemId.PersistentNode(weekendNodeId),
											currentVersion: 0,
											insertIndex: 0
										)
									)
								)
							);
						}
						if (weeklyEvent != default) {
							// Move the event to the Weekly row
							await EntityAskAsync(
								LiveOpsTimelineManager.EntityId,
								new InvokeLiveOpsTimelineCommandRequest(
									new MoveItemsCommand(
										[
											new MoveItemsCommand.ItemMove(
												new ItemId.Element(
													new LiveOpsEventTimelineDataSource.LiveOpsEventId(
														occurrence.OccurrenceId
													)
												),
												currentVersion: 0,
												parentVersion: 0
											)
										],
										new MoveItemsCommand.NewParentInfo(
											new ItemId.PersistentNode(weeklyNodeId),
											currentVersion: 0,
											insertIndex: 0
										)
									)
								)
							);
						}
						if (battlePassEvent != default) {
							// Move the event to the Battle Pass row
							await EntityAskAsync(
								LiveOpsTimelineManager.EntityId,
								new InvokeLiveOpsTimelineCommandRequest(
									new MoveItemsCommand(
										[
											new MoveItemsCommand.ItemMove(
												new ItemId.Element(
													new LiveOpsEventTimelineDataSource.LiveOpsEventId(
														occurrence.OccurrenceId
													)
												),
												currentVersion: 0,
												parentVersion: 0
											)
										],
										new MoveItemsCommand.NewParentInfo(
											new ItemId.PersistentNode(battlePassNodeId),
											currentVersion: 0,
											insertIndex: 0
										)
									)
								)
							);
						}
					} catch (Exception ex) {
						_log.LogEvent(LogLevel.Warning, ex, "Failed to move event {EventId}", occurrence.OccurrenceId);
					}
				}
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}
	}
}
