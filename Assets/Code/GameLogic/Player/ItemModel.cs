using System;
using Metaplay.Core;
using Metaplay.Core.Math;
using Metaplay.Core.Model;

namespace Game.Logic {
	/// <summary>
	/// ItemLockedState describes a property of an item (usually a creator) that restricts how the
	/// item can be interacted with. An example could be a chest that is initially <c>Closed</c> (locked).
	/// When the user taps "Open" a timer (e.g. 30 minutes) is started. The item is now in <c>Opening</c>
	/// state and after the timer is up will become <c>Open</c> (at which point items can be spawn from
	/// the creator).
	/// </summary>
	[MetaSerializable]
	public enum ItemLockedState {
		Open,
		Opening,
		Closed
	}

	/// <summary>
	/// <para>ItemState describes the state of an item concerning how the item can be interacted with.
	/// The item states are most obvious when exploring a new island. Loosely speaking, the <c>Free</c>
	/// area on the board might be surrounded with <c>FreeForMerge</c> creator items. When a creator
	/// is emptied (and disappears) the states of the surrounding items change: radial <c>Hidden</c>
	/// neighbors become <c>FreeForMerge</c> or <c>Free</c> (if configured as <c>SkipFreeForMerge</c>;
	/// diagonal <c>Hidden</c> items become <c>PartiallyVisible</c>.</para>
	///
	/// <para>Therefore, an item can be thought of progressing through item states: <c>Hidden</c> ->
	/// <c>PartiallyVisible</c> -> <c>FreeForMerge</c> -> <c>Free</c> (potentially skipping some
	/// states). NOTE! At the moment <c>PartiallyVisible</c> behaves technically the same as <c>Hidden</c>.
	/// </para>
	/// </summary>
	[MetaSerializable]
	public enum ItemState {
		/// <summary>Item can be moved and merged freely.</summary>
		Free,
		/// <summary>Item cannot be moved but can be merged with another <c>Free</c> (and thus movable) item.
		/// Also, items can be spawned from a <c>FreeForMerge</c> creator.</summary>
		FreeForMerge,
		/// <summary>Item is <c>PartiallyVisible</c> when it has no <c>Free</c> radial neighbors but
		/// at least one <c>Free</c> diagonal neighbor.</summary>
		PartiallyVisible,
		/// <summary>Item is hidden when none of its neighbors (radial or diagonal) is free.</summary>
		Hidden
	}

	[MetaSerializable]
	public enum ItemBuildState {
		NotStarted,
		Building,
		PendingComplete,
		Complete
	}

	[MetaSerializable]
	public class ItemModel {
		[MetaMember(1)] public ChainInfo Info { get; private set; }
		[MetaMember(2)] public int X { get; private set; }
		[MetaMember(3)] public int Y { get; private set; }
		[MetaMember(4)] public CreatorModel Creator { get; private set; }
		[MetaMember(5)] public ConverterModel Converter { get; private set; }
		[MetaMember(6)] public MetaTime OpenAt { get; private set; }
		[MetaMember(7)] public ItemLockedState LockedState { get; private set; }
		[MetaMember(8)] public ItemState State { get; set; }
		[MetaMember(9)] public bool SkipFreeForMerge { get; set; }
		[MetaMember(10)] public bool Bubble { get; set; }
		[MetaMember(11)] public ItemBuildState BuildState { get; set; }
		[MetaMember(12)] public int BuilderId { get; set; }
		[MetaMember(13)] public MineModel Mine { get; private set; }
		[MetaMember(14)] public BoosterModel Booster { get; private set; }
		[MetaMember(15)] public MetaTime Discovered { get; private set; }

		public override string ToString() {
			return $"{Info.Type.Value}:{Info.Level} ({X},{Y})";
		}

		public bool HasItems => Creator?.ItemCount > 0 || Converter?.HasItem == true;
		public bool IsUsingBuilder => BuilderId > 0 || Mine?.BuilderId > 0;
		public int UsedBuilderId => BuilderId > 0 ? BuilderId : Mine?.BuilderId ?? 0;

		public F64 ConverterProgress {
			get {
				if (Converter == null) {
					return F64.Zero;
				}

				return F64.FromInt(Converter.CurrentValue) / Converter.Info.Items[Converter.CurrentIndex].Count;
			}
		}

		public ItemModel() { }

		public ItemModel(ChainTypeId type, int level, SharedGameConfig gameConfig, MetaTime currentTime, bool finishBuilding) {
			Info = gameConfig.Chains[new LevelId<ChainTypeId>(type, level)];
			if (Info.CreatorType != CreatorTypeId.None) {
				LevelId<CreatorTypeId> creatorId = new(Info.CreatorType, Info.CreatorLevel);
				CreatorInfo creatorInfo = gameConfig.Creators[creatorId];
				Creator = new CreatorModel(creatorInfo, currentTime);
			}

			if (Info.ConverterType != ConverterTypeId.None) {
				LevelId<ConverterTypeId> converterId = new(Info.ConverterType, Info.ConverterLevel);
				ConverterInfo converterInfo = gameConfig.Converters[converterId];
				Converter = new ConverterModel(converterInfo);
			}

			if (Info.MineType != MineTypeId.None) {
				Mine = new MineModel(gameConfig, Info.MineType);
			}

			if (Info.BoosterType != BoosterTypeId.None) {
				BoosterInfo boosterInfo = gameConfig.Boosters[Info.BoosterType];
				Booster = new BoosterModel(boosterInfo);
			}

			State = ItemState.Free;
			OpenAt = MetaTime.Epoch;
			LockedState = Info.OpenTime > MetaDuration.Zero ? ItemLockedState.Closed : ItemLockedState.Open;
			BuildState = Info.BuildTime > MetaDuration.Zero && !finishBuilding ? ItemBuildState.NotStarted : ItemBuildState.Complete;
		}

		public void SetLocation(int x, int y) {
			X = x;
			Y = y;
		}

		public void Discover(MetaTime now) {
			Discovered = now;
		}

		public bool CanMove => !Bubble && State == ItemState.Free && Info.Movable;

		public bool CanBeDiscovered => !IsDiscovered && (State == ItemState.Free || CanCreate) && !Bubble;

		public bool IsDiscovered => Discovered != MetaTime.Epoch;

		public bool CanSelect => !Bubble && State == ItemState.Free;

		public bool CanCreate =>
			!Bubble &&
			(State == ItemState.Free || State == ItemState.FreeForMerge) &&
			BuildState == ItemBuildState.Complete &&
			Creator != null &&
			!Creator.Info.AutoSpawn;

		public bool HasTimer =>
			!Bubble &&
			(State == ItemState.Free || State == ItemState.FreeForMerge) &&
			Creator != null &&
			!HasItems &&
			(!Creator.Info.Disposable || Creator.Info.WaveCount > 0);

		public bool CanRemove => CanMove && Info.Sellable;

		/// <summary>
		/// <c>CanCollect</c> tells whether an item can be collected on the given island. An item can be collected
		/// in two ways: either the item is associated with currency type (gold, gem, orange, etc.) and (thus when
		/// being collected) is added to user's resources OR the item needs to be sent to its target island
		/// (if not the current island).
		/// </summary>
		/// <param name="currentIsland">island where the item resides currently</param>
		/// <returns>whether the item can be collected</returns>
		public bool CanCollect(IslandTypeId currentIsland) {
			if (Info.TargetIsland == currentIsland || Info.TargetIsland == IslandTypeId.All) {
				// Item with target island "All" should not be sent between islands when tried to be collected.
				// This applies to e.g. max level boosters whereas booster fragments are sent to main island
				// (to be merged further).
				return CanMove && Info.CollectableType != CurrencyTypeId.None;
			} else {
				// (Concrete) target island of the item is not the same as the current island.
				// For example, the target island of gold is MainIsland. Thus, when gold is collected on some other
				// island it is sent to its target island (MainIsland).
				return CanMove || CanCreate;
			}
		}

		public bool IsWildcard => Booster != null && Booster.IsWildcard;
		public bool IsBuilder => Booster != null && Booster.IsBuilder;
		public bool IsPermanentMine => Mine != null && Mine.Info.Disposable == false && Info.Width > 1;

		public bool CanMergeWith(ItemModel other, SharedGameConfig gameConfig) {
			if (LockedState == ItemLockedState.Opening) {
				return false;
			}

			if (!Info.Mergeable) {
				return false;
			}

			if (BuildState != ItemBuildState.Complete || other.BuildState != ItemBuildState.Complete) {
				return false;
			}

			if (CanWildcardMergeWith(other, gameConfig)) {
				return true;
			}

			bool hasMoreLevels = gameConfig.Chains.ContainsKey(new LevelId<ChainTypeId>(Info.Type, Info.Level + 1));
			return Info.ConfigKey.Equals(other.Info.ConfigKey) && hasMoreLevels && LockedState == other.LockedState;
		}

		public ChainInfo GetNextLevelItemInfo(SharedGameConfig gameConfig) {
			LevelId<ChainTypeId> key = new(Info.Type, Info.Level + 1);
			gameConfig.Chains.TryGetValue(key, out ChainInfo chainInfo);
			return chainInfo;
		}

		private bool CanWildcardMergeWith(ItemModel other, SharedGameConfig gameConfig) {
			if (IsWildcard && !Info.ConfigKey.Equals(other.Info.ConfigKey)) {
				bool hasMoreLevels =
					gameConfig.Chains.ContainsKey(new LevelId<ChainTypeId>(other.Info.Type, other.Info.Level + 1));
				return LockedState == ItemLockedState.Open &&
					hasMoreLevels &&
					other.Info.Level <= Booster.Info.MaxWildcardLevel;
			}

			return false;
		}

		public bool CanApplyBuilderTo(ItemModel other) {
			return IsBuilder &&
				other.BuildState != ItemBuildState.Complete &&
				other.BuildState != ItemBuildState.PendingComplete;
		}

		private void SetBubble(SharedGameConfig gameConfig, MetaTime currentTime) {
			Bubble = true;
			OpenAt = currentTime + gameConfig.Global.BubbleTtl;
		}

		public void OpenBubble() {
			Bubble = false;
			OpenAt = MetaTime.Epoch;
		}

		public void StartOpening(MetaTime currentTime) {
			LockedState = ItemLockedState.Opening;
			OpenAt = currentTime + Info.OpenTime;
		}

		public void OpenNow() {
			LockedState = ItemLockedState.Open;
		}

		public MetaDuration OpenTimeLeft(MetaTime currentTime) {
			MetaDuration timeLeft = OpenAt - currentTime;
			return MetaDuration.Max(MetaDuration.Zero, timeLeft);
		}

		public Cost SkipOpenCost(SharedGameConfig gameConfig, MetaTime currentTime) {
			int secondsLeft = F64.CeilToInt(OpenTimeLeft(currentTime).ToSecondsF64());
			TimerCostInfo costInfo = gameConfig.TimerCosts[TimerTypeId.SkipOpenItemTimer];
			return new Cost(costInfo.CurrencyType, costInfo.CalculateCost(secondsLeft));
		}

		public void StartBuilding(int builderId) {
			BuilderId = builderId;
			BuildState = ItemBuildState.Building;
		}

		public void FinishBuilding() {
			BuilderId = 0;
			BuildState = ItemBuildState.PendingComplete;
		}

		public void AcknowledgeBuilding() {
			BuildState = ItemBuildState.Complete;
		}

		public Cost SkipCreatorTimerCost(SharedGameConfig gameConfig, MetaTime currentTime) {
			TimerCostInfo costInfo = gameConfig.TimerCosts[TimerTypeId.SkipCreatorTimer];
			if (Creator == null || Creator.ItemCount > 0) {
				return new Cost(costInfo.CurrencyType, 0);
			}

			int secondsLeft = F64.CeilToInt(Creator.TimeToFill(currentTime).ToSecondsF64());
			return new Cost(costInfo.CurrencyType, costInfo.CalculateCost(secondsLeft));
		}

		public ItemModel TrySpawnBubble(RandomPCG random, SharedGameConfig gameConfig, MetaTime currentTime) {
			LevelId<ChainTypeId> bubbleId = RandomizeBubbleInfo(random);
			if (bubbleId.Type != ChainTypeId.None) {
				ItemModel bubble = new ItemModel(bubbleId.Type, bubbleId.Level, gameConfig, currentTime, false);
				bubble.SetBubble(gameConfig, currentTime);
				return bubble;
			}

			return null;
		}

		public bool CanConvertItem(ItemModel item) {
			if (Converter == null) {
				return false;
			}

			return Converter.CanConvertItem(item);
		}

		private LevelId<ChainTypeId> RandomizeBubbleInfo(RandomPCG random) {
			F64 randomValue = F64.FromInt(random.NextInt(10000)) / 10000;
			F64 totalValue = Info.BubbleProbability;
			if (randomValue < totalValue) {
				return Info.ConfigKey;
			}

			foreach (Spawnable spawnable in Info.OtherBubbleSpawn) {
				totalValue += spawnable.DropRate;
				if (randomValue <= totalValue) {
					return spawnable.ChainId;
				}
			}

			return new LevelId<ChainTypeId>(ChainTypeId.None, 0);
		}

		public void Update(IslandTypeId islandId, MetaTime currentTime, IPlayerModelClientListener clientListener) {
			if (Creator != null) {
				if (Creator.Update(currentTime)) {
					clientListener.OnMergeItemStateChanged(islandId, this);
				}
			}

			if (LockedState == ItemLockedState.Opening && OpenAt <= currentTime) {
				LockedState = ItemLockedState.Open;
				clientListener.OnMergeItemStateChanged(islandId, this);
			}
		}
	}
}
