using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Logic;
using Game.Logic.LiveOpsEvents;
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

		private static List<(int day, int month, int durationInDays, string name)> Events = new() {
			new(2, 11, 1, "All Saints' Day"),
			new(10, 11, 1, "Father's Day"),
			new(6, 12, 1, "Independence Day"),
			new(24, 12, 3, "Christmas"),
			new(31, 12, 2, "New Year's"),
			new(6, 1, 1, "Epiphany"),
			new(14, 2, 1, "Valentine’s Day"),
			new(18, 4, 4, "Easter"),
			new(1, 5, 1, "May Day"),
			new(11, 5, 1, "Mother's Day"),
			new(29, 5, 1, "Ascension Day"),
			new(8, 6, 1, "Whit Sunday"),
			new(20, 6, 2, "Midsummer"),
		};

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

		private async Task CreateLiveOpsEvents() {
			try {
				GetLiveOpsEventsResponse events = await EntityAskAsync<GetLiveOpsEventsResponse>(
					LiveOpsTimelineManager.EntityId,
					new GetLiveOpsEventsRequest(true)
				);

				MetaDatabase db = MetaDatabase.Get();
				PersistedStaticGameConfig staticArchive = await db.TryGetAsync<PersistedStaticGameConfig>(_state.StaticGameConfigId.ToString());
				ConfigArchive staticConfig = ConfigArchive.FromBytes(staticArchive.ArchiveBytes);

				var groupId = events.TimelineStateMaybe.Nodes.FirstOrDefault(x=> x.Value.NodeType == NodeType.Group).Key;

				if (events.TimelineStateMaybe.Nodes.All(x => x.Value.DisplayName != "Weekday")) {
					await EntityAskAsync(
						LiveOpsTimelineManager.EntityId,
						new InvokeLiveOpsTimelineCommandRequest(
							new CreateNewItemCommand(NodeType.Row, new MetaDictionary<ItemMetadataField, string>() { { ItemMetadataField.DisplayName, "Weekday" } }, new ItemId.Node(groupId), parentVersion: 0)
						)
					);
					await EntityAskAsync(
						LiveOpsTimelineManager.EntityId,
						new InvokeLiveOpsTimelineCommandRequest(
							new CreateNewItemCommand(NodeType.Row, new MetaDictionary<ItemMetadataField, string>() { { ItemMetadataField.DisplayName, "Weekend" } }, new ItemId.Node(groupId), parentVersion: 0)
						)
					);
				}
				
				events = await EntityAskAsync(LiveOpsTimelineManager.EntityId, new GetLiveOpsEventsRequest(getTimelineData: true));

				var weekdayNodeId = events.TimelineStateMaybe.Nodes.FirstOrDefault(x=> x.Value.DisplayName == "Weekday").Key;
				var weekendNodeId = events.TimelineStateMaybe.Nodes.FirstOrDefault(x=> x.Value.DisplayName == "Weekend").Key;

				var fullGameConfig = FullGameConfig.CreateSoloUnpatched(staticConfig);
				var sharedGameConfig = (fullGameConfig.SharedConfig as SharedGameConfig);
				var keys = sharedGameConfig.MergeEventTemplates.Keys.ToList();

				var keyCount = keys.Count();

				var lastEventAt = events.Occurrences.MaxBy(x => x?.UtcScheduleOccasionMaybe?.GetEnabledEndTime().MillisecondsSinceEpoch)
						?.UtcScheduleOccasionMaybe?.GetEnabledEndTime() ??
					MetaTime.FromDateTime(new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc));
				
				for (int i = 0; i < 365; i++) {
					var date = (lastEventAt + MetaDuration.FromDays(i)).ToDateTime();

					// Only create events for the next year
					if (date > DateTime.Now.AddDays(365))
						return;
					
					var startDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);

					foreach (var (day, month, durationInDays, name) in Events) {
						if (day == date.Day && month == date.Month) {
							var next = Random.Shared.Next(keyCount + 1);

							string description;
							LiveOpsEventContent content;

							if (next < keyCount) {
								description = "Merge Event!";
								content = sharedGameConfig.MergeEventTemplates[keys[next]].Content;
							} else {
								description = "Double Gems!";
								content = new CurrencyMultiplierEvent() {
									Multiplier = 2,
									Type = CurrencyTypeId.Gems
								};
							}

							var response = await EntityAskAsync<CreateLiveOpsEventResponse>(
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

					if (date.DayOfWeek == DayOfWeek.Saturday) {
						LiveOpsEventContent content = new CurrencyMultiplierEvent() {
							Multiplier = 2,
							Type = CurrencyTypeId.Xp
						};
						var response = await EntityAskAsync<CreateLiveOpsEventResponse>(
							LiveOpsTimelineManager.EntityId,
							new CreateLiveOpsEventRequest(
								validateOnly: false,
								new LiveOpsEventSettings(
									new MetaRecurringCalendarSchedule(
										MetaScheduleTimeMode.Utc,
										MetaCalendarDateTime.FromDateTime(startDate),
										new MetaCalendarPeriod(0, 0, 2, 0, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 12, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 12, 0, 0),
										new MetaCalendarPeriod(),
										null,
										null
									),
									new LiveOpsEventParams(
										"Double Xp",
										"Weekend Double Xp",
										"#ebbf34",
										new List<EntityId>(),
										null,
										null,
										content
									)
								)
							)
						);
						await EntityAskAsync(
							LiveOpsTimelineManager.EntityId,
							new InvokeLiveOpsTimelineCommandRequest(
								new MoveItemsCommand(
									new List<MoveItemsCommand.ItemMove>() { new(new ItemId.Element(new ElementId.LiveOpsEvent(response.InitialEventOccurrenceId.Value)), currentVersion: 0, parentVersion: 0) },
									new MoveItemsCommand.NewParentInfo(new ItemId.Node(weekendNodeId), currentVersion: 0, insertIndex: 1)
								)
							)
						);
					}
					if (date.DayOfWeek == DayOfWeek.Monday) {
						var next = Random.Shared.Next(keyCount);
						LiveOpsEventContent content = sharedGameConfig.MergeEventTemplates[keys[next]].Content;
						var response = await EntityAskAsync<CreateLiveOpsEventResponse>(
							LiveOpsTimelineManager.EntityId,
							new CreateLiveOpsEventRequest(
								validateOnly: false,
								new LiveOpsEventSettings(
									new MetaRecurringCalendarSchedule(
										MetaScheduleTimeMode.Utc,
										MetaCalendarDateTime.FromDateTime(startDate),
										new MetaCalendarPeriod(0, 0, 5, 0, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 12, 0, 0),
										new MetaCalendarPeriod(0, 0, 0, 12, 0, 0),
										new MetaCalendarPeriod(),
										null,
										null
									),
									new LiveOpsEventParams(
										"Merge Event",
										"Weekday Merge Event",
										"#3f6730",
										new List<EntityId>(),
										null,
										null,
										content
									)
								)
							)
						);
						var rsp = await EntityAskAsync(
							LiveOpsTimelineManager.EntityId,
							new InvokeLiveOpsTimelineCommandRequest(
								new MoveItemsCommand(
									new List<MoveItemsCommand.ItemMove>() { new(new ItemId.Element(new ElementId.LiveOpsEvent(response.InitialEventOccurrenceId.Value)), currentVersion: 0, parentVersion: 0) },
									new MoveItemsCommand.NewParentInfo(new ItemId.Node(weekdayNodeId), currentVersion: 0, insertIndex: 1)
								)
							)
						);
					}
				}
			} catch (Exception ex) {
				_log.LogEvent(LogLevel.Error, ex, "Exception occured");
			}
		}

		public string GetRandomColor() {
			var strings = new string[] {
				"#c4001d",
				"#d97775",
				"#e34a2f",
				"#ebbf34",
				"#3f6730",
				"#4b99e3",
				"#4b4cb3",
				"#7d83c9",
				"#8702a8",
				"#616161",
			};
			
			return strings[new Random().Next(0, strings.Length)];
		}
	}
}
