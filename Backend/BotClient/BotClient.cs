// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Game.Logic;
using Metaplay.BotClient;
using Metaplay.Cloud.Entity;
using Metaplay.Core;
using Metaplay.Core.Client;
using Metaplay.Core.Config;
using Metaplay.Core.InAppPurchase;
using Metaplay.Core.InGameMail;
using Metaplay.Core.Message;
using Metaplay.Core.Model;
using Metaplay.Core.Player;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Game.Logic.TypeCodes;
using Metaplay.Cloud.RuntimeOptions;
using Metaplay.Core.Debugging;
using Metaplay.Core.Guild;
using Metaplay.Core.Guild.Messages.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Network;
using static System.FormattableString;

namespace Game.BotClient {
	public enum BotClientState {
		Connecting,
		Main,
	}

	// BotClient

	[EntityConfig]
	internal class BotClientConfig : BotClientConfigBase {
		public override Type EntityActorType => typeof(BotClient);
	}

	public class BotClient : BotClientBase {
		BotClientState State { get; set; } = BotClientState.Connecting;

		PlayerModel _playerModel => (PlayerModel)_playerContext.Journal.StagedModel;
		DefaultPlayerClientContext _playerContext;
		private OrcaLeagueClient _leagueClient;
		GuildModel GuildModel => (GuildModel)GuildContext?.CommittedModel;

		private bool hasDoneSpecialBotHandling = false;

		protected override IMetaplaySubClient[] AdditionalSubClients =>
			new IMetaplaySubClient[] { _leagueClient = new OrcaLeagueClient(ClientSlotGame.OrcaLeague) };

		protected override TimeSpan UpdateTickInterval =>
			RuntimeEnvironmentInfo.Instance.EnvironmentFamily == EnvironmentFamily.Local
				? TimeSpan.FromSeconds(1)
				: TimeSpan.FromSeconds(30);

		protected override IPlayerClientContext PlayerContext => _playerContext;

		private MetaTime lastPurchase = MetaTime.Epoch;

		public BotClient(EntityId entityId) : base(entityId) { }

		protected override void PreStart() {
			base.PreStart();

			// Schedule session stop in 0.5..1.5 * expectedSessionDuration
			TimeSpan sessionDuration = _botOpts.ExpectedSessionDuration * (0.5 + new Random().NextDouble());
			_log.Debug("Session duration = {SessionDuration}", sessionDuration);
		}

		protected override void RegisterHandlers() {
			base.RegisterHandlers();
		}

		protected override string GetCurrentStateLabel() => State.ToString();

		protected override Task OnUpdate() {
			// Tick current state (when connected)
			switch (State) {
				case BotClientState.Main:
					TickMainState();
					break;
			}

			return Task.CompletedTask;
		}

		protected override Task OnNetworkMessage(MetaMessage message) {
			//_log.Debug("OnNetworkMessage: {Message}", PrettyPrint.Compact(message));
			switch (message) {
				case SessionProtocol.SessionStartSuccess success:
					// HandleStartSession handles the player model setup
					State = BotClientState.Main;
					break;

				case PlayerAckActions ackActions:
					_playerContext.PurgeSnapshotsUntil(
						JournalPosition.FromTickOperationStep(
							ackActions.UntilPositionTick,
							ackActions.UntilPositionOperation,
							ackActions.UntilPositionStep
						)
					);
					break;

				case PlayerExecuteUnsynchronizedServerAction executeUnsynchronizedServerAction:
					_playerContext.ExecuteServerAction(executeUnsynchronizedServerAction);
					break;

				case PlayerChecksumMismatch checksumMismatch:
					// On mismatch, report it and terminate bot (to avoid spamming)
					_log.Warning(
						"PlayerChecksumMismatch: tick={Tick}, actionIndex={ActionIndex}",
						checksumMismatch.Tick,
						checksumMismatch.ActionIndex
					);
					_playerContext.ResolveChecksumMismatch(checksumMismatch);
					RequestShutdown();
					break;

				case PlayerJoinOrcaLeagueResponse _: break;

				default:
					_log.Warning("Unknown message received: {Message}", PrettyPrint.Compact(message));
					break;
			}

			return Task.CompletedTask;
		}

		protected override void HandleStartSession(
			SessionProtocol.SessionStartSuccess success,
			IPlayerModelBase playerModelBase,
			ISharedGameConfig gameConfig
		) {
			PlayerModel playerModel = (PlayerModel)playerModelBase;
			//playerModel.ClientListener = this;

			_playerContext = new DefaultPlayerClientContext(
				_logChannel,
				playerModel,
				success.PlayerState,
				_actualPlayerId,
				_logicVersion,
				timelineHistory: null,
				SendToServer,
				MetaTime.Now
			);
		}

		bool IsSpecialBot() {
			return _playerModel.PlayerId.Value < 5;
		}

		private UnitySystemInfo GenerateMockedSystemInfo()
		{
			return new UnitySystemInfo
			{
				BatteryLevel = 13.37f,
				DeviceModel = "AppleBook Pro Max Plus Ultra",
				DeviceType = "Folding Phablet",
				GraphicsDeviceId = 42424242,
				GraphicsDeviceName = "RTX 9090 Ti Super Mega Edition",
				GraphicsDeviceType = "Dedicated",
				GraphicsDeviceVendor = "Metaplay Silicon Foundries",
				GraphicsDeviceVendorId = 31337,
				GraphicsDeviceVersion = "3.14159",
				GraphicsDeviceMemoryMegabytes = 65536,
				OperatingSystem = "Doors 12.1",
				OperatingSystemFamily = "Macrohard Doors",
				ProcessorCount = 16,
				ProcessorFrequencyMhz = 9001,
				ProcessorType = "Ryzen 15 9999X",
				SystemMemoryMegabytes = 131072,
				ScreenWidth = 3440,
				ScreenHeight = 1440,
				ScreenDPI = 420f,
				ScreenOrientation = "Face Down",
				IsFullScreen = true
			};
		}

		private UnityPlatformInfo GenerateMockedPlatformInfo()
		{
			return new UnityPlatformInfo
			{
				BuildGuid = "MOCK_BUILD_GUID_12345",
				Platform = "SpaceStationOS",
				InternetReachability = "Interstellar",
				IsEditor = false,
				ApplicationVersion = "9.9.9-alpha",
				UnityVersion = "2042.1.0f1",
				SystemLanguage = "Galactic Standard",
				InstallMode = "QuantumDownload",
				IsGenuine = true
			};
		}

		public PlayerUploadIncidentReport CreateSessionStartFailed()
		{
			MetaTime now = MetaTime.Now;

			List<ClientLogEntry> logEntries = new List<ClientLogEntry>();

			RandomPCG rnd = RandomPCG.CreateNew();

			var logicVersionMismatch = new Handshake.LogicVersionMismatch(new MetaVersionRange(1, 5));
			var report = new PlayerIncidentReport.SessionStartFailed(
				sharedIncidentInfo: new PlayerIncidentReport.SharedIncidentInfo() {
					OccurredAt = now,
					ClientLogEntries = logEntries,
					ClientSystemInfo = GenerateMockedSystemInfo(),
					ClientPlatformInfo = GenerateMockedPlatformInfo(),
					ApplicationInfo = new IncidentApplicationInfo(
						buildVersion: GetBuildVersion(),
						deviceGuid: _sessionGuidService.TryGetDeviceGuid(),
						activeLanguage: ActiveLanguage.LanguageId.Value,
						highestSupportedLogicVersion: MetaplayCore.Options.ClientLogicVersion,
						environmentId: "Botclient"
					),
					GameConfigInfo = new IncidentGameConfigInfo(
						ContentHash.None,
						ContentHash.None,
						new List<ExperimentVariantPair>()
					)
				},
				errorType:              "LogicVersionMismatch",
				networkError:           PrettyPrint.Verbose(logicVersionMismatch).ToString(),
				reasonOverride:         null,
				endpoint: new ServerEndpoint(
					_botOpts.ServerHost,
					_botOpts.ServerPort,
					_botOpts.EnableTls,
					_botOpts.CdnBaseUrl,
					null,
					_botOpts.ServerBackupGateways.Select(opt => opt.ToServerGateway()).ToList()),
				networkReachability:    "ReachableViaCarrierDataNetwork",
				networkReport:          NetworkDiagnostics.CreateDummyReport(),
				tlsPeerDescription:     "tcp");

			return new PlayerUploadIncidentReport(
				report.IncidentId,
				PlayerIncidentUtil.CompressIncidentForNetworkDelivery(report));
		}

        public PlayerUploadIncidentReport CreateIncidentReport()
        {
            MetaTime now = MetaTime.Now;

            List<ClientLogEntry> logEntries = new List<ClientLogEntry>();
			var stackTrace = new StackTrace().ToString();
            for (int i = 0; i < 1; i++) {
				logEntries.Add(new ClientLogEntry(
                    timestamp: now - MetaDuration.FromSeconds(1),
                    level: ClientLogEntryType.Log,
                    message: Invariant($"Method 'CreateAnalyticsEvent' not implemented for bots."),
                    stackTrace: stackTrace));
			}

            RandomPCG rnd = RandomPCG.CreateNew();
			
            PlayerIncidentReport report = new PlayerIncidentReport.UnhandledExceptionError(
                id: PlayerIncidentUtil.EncodeIncidentId(now, rnd.NextULong()),
                occurredAt: now,
                logEntries: logEntries,
                systemInfo: GenerateMockedSystemInfo(),
                platformInfo: GenerateMockedPlatformInfo(),
                applicationInfo: new IncidentApplicationInfo(
                    buildVersion:                   GetBuildVersion(),
                    deviceGuid:                     _sessionGuidService.TryGetDeviceGuid(),
                    activeLanguage:                 ActiveLanguage.LanguageId.Value,
                    highestSupportedLogicVersion:   MetaplayCore.Options.ClientLogicVersion,
                    environmentId:                  "Botclient"),
                gameConfigInfo: new IncidentGameConfigInfo(ContentHash.None, ContentHash.None, new List<ExperimentVariantPair>()),
                exceptionName: "System.InvalidOperationException",
                exceptionMessage: $"Method 'CreateAnalyticsEvent' not implemented for bots.",
                stackTrace: stackTrace);

            return new PlayerUploadIncidentReport(
                report.IncidentId,
                PlayerIncidentUtil.CompressIncidentForNetworkDelivery(report));
        }

		void HandleSpecialBot() {
			if (_playerModel.Level.CurrentXp > 0)
				return;
			
			switch (_playerModel.PlayerId.Value) {
				case 0:
					SendToServer(new PlayerChangeOwnNameRequest("Unhandled Exception User"));
					SendToServer(CreateIncidentReport());
					break;
				case 1:
					SendToServer(new PlayerChangeOwnNameRequest("Checksum Incident User"));
					_playerContext.ExecuteAction(new PlayerForceChecksumMismatch(IslandTypeId.MainIsland));
					break;
				case 2:
					SendToServer(new PlayerChangeOwnNameRequest("Session Start Failed User"));
					SendToServer(CreateSessionStartFailed());
					break;
				case 3:  break;
				case 4:  break;
				default: break;
			}

			hasDoneSpecialBotHandling = true;
		}
		
		void TickMainState() {
			
			if (IsSpecialBot() && !hasDoneSpecialBotHandling && _playerModel.Stats.TotalLogins <= 1) {
				HandleSpecialBot();
			}

			RandomPCG rnd = RandomPCG.CreateNew();

			_log.Debug(
				"Tick player ({id}) (currentTick={CurrentTick})..",
				_playerModel.PlayerId,
				_playerModel.CurrentTick
			);

			UpdateGuild();

			if (_playerModel.DivisionClientState?.CurrentDivision == EntityId.None) {
				_leagueClient.TryJoinLeagues();
			}

			if (_playerModel.Wallet.Gems.Value < 50 &&
				lastPurchase < MetaTime.Now + MetaDuration.FromMinutes(-5) &&
				(!_playerModel.InAppPurchaseHistory.Any() ||
					_playerModel.InAppPurchaseHistory.Last().PurchaseTime.ToDateTime() <
					DateTime.UtcNow.Subtract(new TimeSpan(0, 15, 0)))) {
				var keyValuePairs = _playerModel.GameConfig.InAppProducts.Where(
					x => x.Value.Resources.FirstOrDefault(x => x.Type == CurrencyTypeId.Gems)?.Amount > 0
				).ToList();
				
				var key = keyValuePairs[Random.Shared.Next(keyValuePairs.Count)].Key;

				lastPurchase = MetaTime.Now;

				StartFakeInAppPurchase(key);
			}

			foreach ((HeroTypeId heroTypeId, var heroModel) in _playerModel.Heroes.Heroes) {
				if(heroModel.CurrentTask?.Info == null)
					continue;
				
				if (_playerModel.Inventory.HasEnoughResources(heroModel.CurrentTask.Info) &&
					heroModel.CurrentTask.State != HeroTaskState.Fulfilled) {
					_playerContext.ExecuteAction(new PlayerFulfillHeroTask(heroTypeId));
				}

				if (heroModel.CurrentTask.State == HeroTaskState.Fulfilled) {
					var cost = _playerModel.SkipHeroTaskTimerCost(heroTypeId);
					if (_playerModel.Wallet.Currency(cost.Type).Value > cost.Amount) {
						_playerContext.ExecuteAction(new PlayerSkipHeroTaskTimer(heroTypeId));
					}
				}

				if (heroModel.CurrentTask?.State == HeroTaskState.Finished) {
					MergeBoardModel mergeBoard = _playerModel.Islands[IslandTypeId.MainIsland].MergeBoard;
					ItemModel heroBuilding = mergeBoard.FindItem(i => i.Info.Type == heroModel.Building);
					if (heroBuilding != null) {
						_playerContext.ExecuteAction(new PlayerClaimHeroTaskRewards(heroTypeId));
					}
				}
			}

			foreach (var (key, value) in _playerModel.Logbook.Chapters) {
				foreach (var (logbookTaskId, logbookTaskModel) in value.Tasks) {
					if (logbookTaskModel.IsComplete && !logbookTaskModel.IsClaimed) {
						_playerContext.ExecuteAction(new PlayerClaimLogbookTaskReward(logbookTaskId));
					}
				}

				if (value.State == ChapterState.Opening) {
					_playerContext.ExecuteAction(new PlayerOpenLogbookChapter(key));
				}

				if (value.State == ChapterState.Complete) {
					_playerContext.ExecuteAction(new PlayerClaimLogbookChapterReward(key));
				}
			}

			foreach (IslandModel island in _playerModel.Islands.Values) {
				if (island.State == IslandState.Revealing) {
					_playerContext.ExecuteAction(new PlayerRevealIsland(island.Info.Type));
				}

				if (island.State == IslandState.Locked &&
					_playerModel.Wallet.EnoughCurrency(island.Info.UnlockCost.Type, island.Info.UnlockCost.Amount)) {
					_playerContext.ExecuteAction(new PlayerUnlockIsland(island.Info.Type));
				}

				if (island.State != IslandState.Open) {
					continue;
				}

				// No need to do this very often, mostly just updates things that might lag behind
				if (Random.Shared.Next(10) <= 1) {
					_playerContext.ExecuteAction(new PlayerEnterIsland(island.Info.Type));

					for (var i = 0; i < _playerModel.Rewards.Count; i++) {
						_playerContext.ExecuteAction(new PlayerClaimReward(island.Info.Type));
					}
				}

				MergeBoardModel board = island.MergeBoard;
				if (board == null)
					return;

				if (island.Tasks != null) {
					foreach (var (key, value) in island.Tasks.Tasks.ToMetaDictionary()) {
						if (board.HasItemsForTask(value.Info))
							_playerContext.ExecuteAction(new PlayerFulfillIslandTask(island.Info.Type, key));
					}
				}

				foreach (var kvp in board.LockArea.Areas) {
					if (kvp.Value == AreaState.Opening) {
						LockAreaInfo areaInfo = _playerModel.GameConfig.LockAreas[new LockAreaId(
							island.Info.Type,
							kvp.Key.ToString()
						)];

						if (areaInfo.UnlockProduct != null &&
							areaInfo.UnlockProduct != InAppProductId.FromString("None") &&
							_playerModel.InAppPurchaseHistory.All(x => x.ProductId != areaInfo.UnlockProduct) &&
							lastPurchase < MetaTime.Now + MetaDuration.FromMinutes(-5)) {
							lastPurchase = MetaTime.Now;
							StartFakeInAppPurchase(areaInfo.UnlockProduct);
							continue;
						}

						if (areaInfo.UnlockCost.Amount == 0 ||
							(_playerModel.Wallet.EnoughCurrency(
								areaInfo.UnlockCost.Type,
								areaInfo.UnlockCost.Amount
							))) {
							if (areaInfo.UnlockProduct == null ||
								areaInfo.UnlockProduct == InAppProductId.FromString("None") ||
								_playerModel.InAppPurchaseHistory.Any(x => x.ProductId == areaInfo.UnlockProduct)) {
								var result =
									_playerContext.ExecuteAction(new PlayerOpenLockArea(island.Info.Type, kvp.Key));
								if (result == MetaActionResult.Success)
									return;
							}
						}
					}
				}

				(int BoardWidth, int BoardHeight) islandSize = (board.Info.BoardWidth, board.Info.BoardHeight);

				List<ChainTypeId> itemsFreeForMerge = new List<ChainTypeId>();
				Dictionary<ChainTypeId, int> requiredTaskItems = new Dictionary<ChainTypeId, int>();
				Dictionary<LevelId<ChainTypeId>, int> currentTaskItems = new Dictionary<LevelId<ChainTypeId>, int>();

				foreach (var item in island.MergeBoard.Items) {
					if (item.State == ItemState.FreeForMerge) {
						itemsFreeForMerge.Add(item.Info.Type);
					}
				}

				if (island.Tasks?.Tasks != null) {
					foreach (var (_, value) in island.Tasks.Tasks) {
						foreach (var itemCountInfo in value.Info.Items) {
							if (!requiredTaskItems.ContainsKey(itemCountInfo.Type))
								requiredTaskItems.Add(itemCountInfo.Type, itemCountInfo.Count);
							else
								requiredTaskItems[itemCountInfo.Type] += itemCountInfo.Count;

							if (!currentTaskItems.ContainsKey(itemCountInfo.ChainId))
								currentTaskItems.Add(itemCountInfo.ChainId, itemCountInfo.Count);
							else
								currentTaskItems[itemCountInfo.ChainId] += itemCountInfo.Count;
						}
					}
				}

				for (int x = 0; x <= islandSize.BoardWidth; x++) {
					for (int y = 0; y <= islandSize.BoardHeight; y++) {
						if (board[x, y] != null &&
							board[x, y].HasItem) {
							var item = board[x, y].Item;
							
							if (item.State != ItemState.Free)
								continue;
							
							if (requiredTaskItems.TryGetValue(item.Info.Type, out int _)) {
								if (island.Tasks.IsItemUsed(new LevelId<ChainTypeId>(item.Info.Type, item.Info.Level)))
									requiredTaskItems[item.Info.Type]--;
							}
							if (currentTaskItems.TryGetValue(item.Info.ConfigKey, out int _)) {
								if (island.Tasks.IsItemUsed(new LevelId<ChainTypeId>(item.Info.Type, item.Info.Level)))
									currentTaskItems[item.Info.ConfigKey]--;
							}
						}
					}
				}

				for (int x = 0; x <= islandSize.BoardWidth; x++) {
					for (int y = 0; y <= islandSize.BoardHeight; y++) {
						if (board[x, y] != null &&
							board[x, y].HasItem) {
							var item = board[x, y].Item;

							if (item == null)
								continue;

							if (item.State != ItemState.Free && item.State != ItemState.FreeForMerge)
								continue;

							if (item.BuildState == ItemBuildState.NotStarted && _playerModel.Builders.Free > 0) {
								_playerContext.ExecuteAction(
									new PlayerUseBuilder(island.Info.Type, x, y)
								);
							}

							if (item.IsUsingBuilder) {
								Cost cost = _playerModel.SkipBuilderTimerCost(item.UsedBuilderId);

								if (_playerModel.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
									_playerContext.ExecuteAction(
										new PlayerSkipBuilderTimer(island.Info.Type, x, y)
									);
								}
							}

							if (item.BuildState == ItemBuildState.PendingComplete) {
								_playerContext.ExecuteAction(
									new PlayerAcknowledgeBuilding(island.Info.Type, x, y)
								);
							}

							if (item.Creator != null &&
								item.Creator.ItemCount == 0 &&
								island.Info.Type != IslandTypeId.EnergyIsland) {
								if (SpawnsUsefulItem(item.Info, island, itemsFreeForMerge, requiredTaskItems, currentTaskItems)) {
									Cost cost = item.SkipCreatorTimerCost(
										_playerModel.GameConfig,
										_playerModel.CurrentTime
									);

									if (_playerModel.Wallet.EnoughCurrency(cost.Type, cost.Amount)) {
										_playerContext.ExecuteAction(
											new PlayerSkipCreatorTimer(island.Info.Type, x, y)
										);
									}
								}
							}

							if (board.CanMoveFrom(x, y) && item.LockedState == ItemLockedState.Closed) {
								_playerContext.ExecuteAction(
									new PlayerOpenMergeItem(island.Info.Type, x, y)
								);
							}

							if (item.CanCollect(island.Info.ConfigKey)) {
								_playerContext.ExecuteAction(
									new PlayerCollectMergeItem(island.Info.Type, x, y)
								);
							}

							if (item.Mine != null &&
								(!item.Mine.Info.RequiresBuilder ||
									item.Mine.Info.RequiresBuilder && _playerModel.Builders.Free > 0) &&
								item.Mine.State == MineState.Idle &&
								_playerModel.Merge.Energy.ProducedAtUpdate > item.Mine.EnergyUsage) {
								if (SpawnsUsefulItem(item.Info, island, itemsFreeForMerge, requiredTaskItems, currentTaskItems))
									_playerContext.ExecuteAction(new PlayerUseMine(island.Info.Type, x, y));
							}

							if (item.Mine != null && item.Mine.State == MineState.NeedsRepair) {
								_playerContext.ExecuteAction(new PlayerRepairMine(island.Info.Type, x, y));
							}

							if (item.Mine != null &&
								item.Mine.State == MineState.ItemsComplete &&
								item.Mine.Queue.Count > 0 &&
								board.FindClosestFreeTile(x, y) != null) {
								if (SpawnsUsefulItem(item.Info, island, itemsFreeForMerge, requiredTaskItems, currentTaskItems))
									_playerContext.ExecuteAction(new PlayerClaimMinedItems(island.Info.Type, x, y));
							}

							if (item.HasItems && item.CanCreate && item.LockedState == ItemLockedState.Open) {
								bool hasEnergy = _playerModel.Merge.Energy.ProducedAtUpdate >=
									item.Creator?.Info?.EnergyUsage;
								if (!hasEnergy) {
									EnergyCostInfo costInfo =
										_playerModel.Merge.EnergyFill.EnergyCost(_playerModel.GameConfig);
									if (_playerModel.Wallet.Currency(costInfo.CurrencyType).Value >= costInfo.Cost) {
										_playerContext.ExecuteAction(new PlayerFillEnergy(island.Info.Type));
										hasEnergy = true;
									}
								}

								if (board.FindClosestFreeTile(x, y) != null &&
									hasEnergy) {
									if (SpawnsUsefulItem(item.Info, island, itemsFreeForMerge, requiredTaskItems, currentTaskItems))
										_playerContext.ExecuteAction(
											new PlayerCreateMergeItem(island.Info.Type, x, y)
										);
								}
							}

							if (island.CanClaimAsBuilding(_playerModel.GameConfig, item) &&
								island.BuildingState != BuildingState.NotRevealed) {
								_playerContext.ExecuteAction(
									new PlayerClaimBuildingFragment(island.Info.Type, x, y)
								);
							}

							if (item.Bubble) {
								int cost = item.Info.BubblePrice;
								if (_playerModel.Wallet.EnoughCurrency(CurrencyTypeId.Gems, cost)) {
									_playerContext.ExecuteAction(
										new PlayerOpenBubble(island.Info.Type, x, y)
									);
								}
							}

							if (item.State == ItemState.Free) {
								for (int toX = 0; toX <= islandSize.BoardWidth; toX++) {
									for (int toY = 0; toY <= islandSize.BoardHeight; toY++) {
										if (x == toX && y == toY)
											continue;

										var result = island.MoveResult(
											_playerModel.GameConfig,
											item,
											toX,
											toY
										);

										var targetTile = board[toX, toY];
										var canMoveFrom = board.CanMoveFrom(x, y);
										if (result != MergeBoardModel.MoveResultType.Building &&
											result != MergeBoardModel.MoveResultType.Invalid &&
											result != MergeBoardModel.MoveResultType.Move &&
											canMoveFrom &&
											targetTile != null &&
											targetTile.HasItem &&
											(targetTile.Item.State == ItemState.FreeForMerge ||
												targetTile.Item.State == ItemState.Free)) {
											int highestSourceTaskItemLevel =
												island.Tasks?.GetHighestUsedItemLevel(item.Info.Type) ?? -1;
											int highestTargetTaskItemLevel =
												island.Tasks?.GetHighestUsedItemLevel(targetTile.Item.Info.Type) ?? -1;

											if (((highestSourceTaskItemLevel == -1 &&
														highestTargetTaskItemLevel == -1) ||
													highestSourceTaskItemLevel > item.Info.Level)) {
												_playerContext.ExecuteAction(
													new PlayerMoveItemOnBoard(
														island.Info.Type,
														x,
														y,
														toX,
														toY
													)
												);
											}
										} else if (result == MergeBoardModel.MoveResultType.Move &&
													targetTile != null &&
													board[x, y].Type != TileType.Ground &&
													targetTile.IsFree &&
													canMoveFrom) {
											_playerContext.ExecuteAction(
												new PlayerMoveItemOnBoard(
													island.Info.Type,
													x,
													y,
													toX,
													toY
												)
											);
										}
									}
								}
							}
						}
					}
				}

				if (board.FindClosestFreeTile(0, 0) == null) {
					bool soldAnything = false;
					int initialMaxLevel = 3;
					soldAnything = TrySellItems(initialMaxLevel, islandSize, board, island, itemsFreeForMerge, requiredTaskItems);

					if (!soldAnything) {
						TrySellItems(initialMaxLevel, islandSize, board, island, itemsFreeForMerge, requiredTaskItems, true);
					}
				}
			}
		}

		private bool TrySellItems(
			int initialMaxLevel,
			(int BoardWidth, int BoardHeight) islandSize,
			MergeBoardModel board,
			IslandModel island,
			List<ChainTypeId> itemsFreeForMerge,
			Dictionary<ChainTypeId, int> requiredTaskItems,
			bool prioritizeTasks = false
		) {
			bool soldAnything = false;
			do {
				Dictionary<LevelId<ChainTypeId>, int> currentTaskItems = new Dictionary<LevelId<ChainTypeId>, int>();
				if (island.Tasks?.Tasks != null) {
					foreach (var (_, value) in island.Tasks.Tasks) {
						foreach (var itemCountInfo in value.Info.Items) {
							if (!currentTaskItems.ContainsKey(itemCountInfo.ChainId))
								currentTaskItems.Add(itemCountInfo.ChainId, itemCountInfo.Count);
							else
								currentTaskItems[itemCountInfo.ChainId] += itemCountInfo.Count;
						}
					}
				}
				
				initialMaxLevel++;
				for (int x = 0; x <= islandSize.BoardWidth; x++) {
					for (int y = 0; y <= islandSize.BoardHeight; y++) {
						if (board[x, y] != null &&
							board[x, y].HasItem) {
							var item = board[x, y].Item;

							if (!item.Info.Sellable || !item.CanMove)
								continue;

							if (IsUsefulItem(item, island, itemsFreeForMerge, requiredTaskItems, currentTaskItems, prioritizeTasks)) {
								continue;
							}

							soldAnything = true;
							_playerContext.ExecuteAction(new PlayerSellMergeItem(island.Info.Type, x, y));
						}
					}
				}

				if (initialMaxLevel >= 15)
					break;
			} while (!soldAnything);

			return soldAnything;
		}

		private void UpdateGuild() {
			Random random = new Random();
			
			if (!GuildClient.HasOngoingGuildDiscovery && random.NextDouble() < 0.005)
			{
				GuildClient.DiscoverGuilds(OnGuildDiscoveryResponse);
			}
		}

		private void OnGuildDiscoveryResponse(GuildDiscoveryResponse obj) {
			if (obj.GuildInfos.Count == 0 ||
				(obj.GuildInfos.All(x => x.NumMembers >= x.MaxNumMembers / 2) &&
					obj.GuildInfos.All(x => x.NumMembers != x.MaxNumMembers) &&
					Random.Shared.Next(5) == 0))
				CreateGuild();
			else if (obj.GuildInfos.Count > 0)
				DefaultHandleGuildDiscoveryResult(obj);
		}

		private void CreateGuild() {
			GuildCreationRequestParams creationParams = new GuildCreationRequestParams();
			creationParams.DisplayName = GenerateRandomGuildName();
			creationParams.Description = GenerateRandomGuildDescription();
			GuildRequirementsValidator guildRequirements = IntegrationRegistry.Get<GuildRequirementsValidator>();
			if (guildRequirements.ValidateDisplayName(creationParams.DisplayName)
				&& guildRequirements.ValidateDescription(creationParams.Description))
			{
				GuildClient.BeginCreateGuild(creationParams, onCompletion: null);
			}
		}

		private bool IsUsefulItem(
			ItemModel item,
			IslandModel island,
			List<ChainTypeId> freeForMergeItems,
			Dictionary<ChainTypeId, int> completedTaskItems,
			Dictionary<LevelId<ChainTypeId>, int> currentTaskItems,
			bool prioritizeTasks = false
		) {
			if (SpawnsUsefulItem(item.Info, island, freeForMergeItems, completedTaskItems, currentTaskItems, prioritizeTasks)) {
				return true;
			}

			if (currentTaskItems.TryGetValue(item.Info.ConfigKey, out var count)) {
				currentTaskItems[item.Info.ConfigKey]--;
				return count > 0;
			}

			int highestSourceTaskItemLevel = island.Tasks?.GetHighestUsedItemLevel(item.Info.Type) ?? -1;

			if (highestSourceTaskItemLevel >= item.Info.Level) {
				return true;
			}

			return false;
		}

		private bool SpawnsUsefulItem(
			ChainInfo item,
			IslandModel island,
			List<ChainTypeId> freeForMergeItems,
			Dictionary<ChainTypeId, int> completedTaskItems,
			Dictionary<LevelId<ChainTypeId>, int> currentTaskItems,
			bool prioritizeTasks = false
		) {
			var maxItemLevel = _playerModel.GameConfig.ChainMaxLevels.GetMaxLevel(item.Type);
			var gameConfigChain =
				_playerModel.GameConfig.Chains[new LevelId<ChainTypeId>(item.Type, maxItemLevel)];

			if (gameConfigChain.CreatorType != CreatorTypeId.None) {
				if (gameConfigChain.CreatorType == CreatorTypeId.FromString("HeroTask") || 
					gameConfigChain.CreatorType == CreatorTypeId.FromString("HeroChest") ||
					gameConfigChain.CreatorType == CreatorTypeId.FromString("Rewards"))
					return true;
				foreach (var spawnable in _playerModel.GameConfig
							.Creators[new LevelId<CreatorTypeId>(gameConfigChain.CreatorType, 1)].Spawnables) {
					if (IsSpawnableUseful(spawnable, island, freeForMergeItems, completedTaskItems, currentTaskItems, prioritizeTasks)) {
						return true;
					}
				}
			} else if (gameConfigChain.MineType != MineTypeId.None) {
				var maxLevel = _playerModel.GameConfig.MineMaxLevels.GetMaxLevel(gameConfigChain.MineType);
				for (int i = 0; i < maxLevel + 1; i++) {
					if (_playerModel.GameConfig.Mines.TryGetValue(
							new LevelId<MineTypeId>(gameConfigChain.MineType, i),
							out var mine
						)) {
						foreach (var spawnable in mine.Items) {
							if (IsSpawnableUseful(spawnable, island, freeForMergeItems, completedTaskItems, currentTaskItems, prioritizeTasks)) {
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		private bool IsSpawnableUseful(
			Spawnable item,
			IslandModel island,
			List<ChainTypeId> freeForMergeItems,
			Dictionary<ChainTypeId, int> requiredTaskItems,
			Dictionary<LevelId<ChainTypeId>, int> currentTaskItems,
			bool prioritizeTasks = false
		) {
			if (!_playerModel.GameConfig.Chains.ContainsKey(item.ChainId))
				return false;
			
			var chain = _playerModel.GameConfig.Chains[item.ChainId];

			if (chain.CollectableType != CurrencyTypeId.None)
				return true;

			if (requiredTaskItems.TryGetValue(item.Type, out int _)) {
				foreach (var (key, value) in currentTaskItems) {
					if (key.Type == item.Type && value > 0)
						return key.Level >= item.Level;
					else if (value > 0)
						return false;
				}
			}
			else if (island.Tasks?.GetHighestUsedItemLevel(item.Type) >= item.Level) {
				return true;
			}

			if (prioritizeTasks)
				return false;

			if (freeForMergeItems.Contains(item.Type)) {
				return true;
			}

			int unfinishedCount = island.GetUnfinishedFragmentCount(_playerModel.GameConfig, item.Type);
			if ((island.BuildingState == BuildingState.Revealed || island.BuildingState == BuildingState.Started) && unfinishedCount > 0) {
				return true;
			}

			return SpawnsUsefulItem(chain, island, freeForMergeItems, requiredTaskItems, currentTaskItems);
		}

		[MessageHandler]
		void HandleInitializeBot(BotCoordinator.InitializeBot _) {
			_log.Debug("Initializing bot");
		}
	}
}
