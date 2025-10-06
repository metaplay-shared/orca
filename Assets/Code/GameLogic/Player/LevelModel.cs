using System;
using System.Runtime.Serialization;
using Metaplay.Core.Analytics;
using Metaplay.Core.Model;
using Metaplay.Core.Player;

namespace Game.Logic {
	[MetaSerializable]
	public abstract class LevelModel : IEquatable<LevelModel>, IComparable<LevelModel> {
		[MetaMember(1)] public int Level { get; set; }
		[MetaMember(2)] public int CurrentXp { get; private set; }
		[MetaMember(3)] public int TotalXp { get; private set; }

		public LevelModel() {
			Level = 1;
			CurrentXp = TotalXp = 0;
		}

		public void AddXp(
			SharedGameConfig gameConfig,
			int delta,
			Action<RewardModel> rewardHandler,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		) {
			if (delta == 0) {
				return;
			}

			int totalDelta = 0;
			TotalXp += delta;
			while (CurrentXp + delta >= GetLevelInfo(gameConfig).XpToNextLevel) {
				if (HasNextLevel(gameConfig)) {
					int diff = GetLevelInfo(gameConfig).XpToNextLevel - CurrentXp;
					delta -= diff;
					totalDelta += diff;
					CurrentXp = 0;
					Level++;
					RewardModel rewards = CreateLevelUpRewards(GetLevelInfo(gameConfig));
					rewardHandler.Invoke(rewards);
					OnLevelUp(rewards, listener);
				} else {
					CurrentXp = GetLevelInfo(gameConfig).XpToNextLevel;
					delta = 0;
					break;
				}
			}
			CurrentXp += delta;
			totalDelta += delta;
			OnXpAdded(totalDelta, listener, serverListener, context);
		}

		public void Reset(int level = 1, int currentXp = 0, int totalXp = 0) {
			Level = level;
			CurrentXp = currentXp;
			TotalXp = totalXp;
		}

		private RewardModel CreateLevelUpRewards(ILevelInfo levelInfo) {
			return new RewardModel(
				levelInfo.RewardResources,
				levelInfo.RewardItems,
				ChainTypeId.LevelUpRewards,
				1,
				Metadata
			);
		}

		public int CompareTo(LevelModel other) {
			return TotalXp.CompareTo(other.TotalXp);
		}

		public bool Equals(LevelModel other) {
			return other != null && TotalXp.Equals(other.TotalXp);
		}

		public override string ToString() {
			return $"{GetType().Name}[level: {Level}, xp: {CurrentXp}, totalXp: {TotalXp}]";
		}

		protected abstract RewardMetadata Metadata { get; }
		public abstract ILevelInfo GetLevelInfo(SharedGameConfig gameConfig);
		public abstract bool HasNextLevel(SharedGameConfig gameConfig);
		protected abstract void OnLevelUp(RewardModel rewards, IPlayerModelClientListener listener);
		protected abstract void OnXpAdded(
			int delta,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		);
	}

	[MetaSerializable]
	public class ActivityEventLevelModel : LevelModel {
		[MetaMember(10)]
		public EventId Event { get; private set; }

		public ActivityEventLevelModel() {
			Reset();
		}

		public ActivityEventLevelModel(EventId eventId) : this() {
			Event = eventId;
		}

		new public void Reset(int level = 0, int currentXp = 0, int totalXp = 0) {
			base.Reset(level, currentXp, totalXp);
		}

		protected override RewardMetadata Metadata =>
			new RewardMetadata {
				Type = RewardType.ActivityEventLevel,
				Level = Level,
				Event = Event
			};

		public override ILevelInfo GetLevelInfo(SharedGameConfig gameConfig) {
			return gameConfig.ActivityEventLevels[new LevelId<EventId>(Event, Level)];
		}

		public override bool HasNextLevel(SharedGameConfig gameConfig) {
			return gameConfig.ActivityEventLevels.ContainsKey(new LevelId<EventId>(Event, Level + 1));
		}

		protected override void OnLevelUp(RewardModel rewards, IPlayerModelClientListener listener) {
			listener.OnActivityEventLevelUp(Event, rewards);
		}

		protected override void OnXpAdded(
			int delta,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		) {
			listener.OnActivityEventScoreAdded(Event, Level, delta, context);
			serverListener.OnActivityEventScoreAdded(Event, Level, delta, context);
		}
	}

	[MetaSerializable]
	public class PlayerLevelModel : LevelModel {
		[IgnoreDataMember] public Action<PlayerEventBase> AnalyticsEventHandler;
		protected override RewardMetadata Metadata =>
			new RewardMetadata {
				Type = RewardType.PlayerLevel,
				Level = Level
			};

		public override ILevelInfo GetLevelInfo(SharedGameConfig gameConfig) {
			return gameConfig.PlayerLevels[Level];
		}

		public override bool HasNextLevel(SharedGameConfig gameConfig) {
			return gameConfig.PlayerLevels.ContainsKey(Level + 1);
		}

		protected override void OnLevelUp(RewardModel rewards, IPlayerModelClientListener listener) {
			listener.OnPlayerLevelUp(rewards);
			AnalyticsEventHandler.Invoke(new PlayerLevelUp(Level));
		}

		protected override void OnXpAdded(
			int delta,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		) {
			listener.OnPlayerXpAdded(delta);
			serverListener.OnPlayerXpAdded(delta);
		}
	}

	[MetaSerializable]
	public class IslandLevelModel : LevelModel {
		[MetaMember(10)]
		public IslandTypeId Island { get; private set; }

		public IslandLevelModel() { }

		public IslandLevelModel(IslandTypeId island) {
			Island = island;
		}

		protected override RewardMetadata Metadata =>
			new RewardMetadata {
				Type = RewardType.IslandLevel,
				Level = Level,
				Island = Island
			};

		public override ILevelInfo GetLevelInfo(SharedGameConfig gameConfig) {
			return gameConfig.IslandLevels[new LevelId<IslandTypeId>(Island, Level)];
		}

		public override bool HasNextLevel(SharedGameConfig gameConfig) {
			return gameConfig.IslandLevels.ContainsKey(new LevelId<IslandTypeId>(Island, Level + 1));
		}

		protected override void OnLevelUp(RewardModel rewards, IPlayerModelClientListener listener) {
			listener.OnIslandLevelUp(Island, rewards);
		}

		protected override void OnXpAdded(
			int delta,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		) {
			listener.OnIslandXpAdded(Island, delta);
			serverListener.OnIslandXpAdded(Island, delta);
		}
	}

	[MetaSerializable]
	public class BuildingLevelModel : LevelModel {
		[MetaMember(10)]
		public IslandTypeId Island { get; private set; }

		public BuildingLevelModel() { }

		public BuildingLevelModel(IslandTypeId island) {
			Island = island;
		}

		protected override RewardMetadata Metadata =>
			new RewardMetadata {
				Type = RewardType.BuildingLevel,
				Level = Level,
				Island = Island
			};

		public override ILevelInfo GetLevelInfo(SharedGameConfig gameConfig) {
			return gameConfig.BuildingLevels[new LevelId<IslandTypeId>(Island, Level)];
		}

		public override bool HasNextLevel(SharedGameConfig gameConfig) {
			return gameConfig.BuildingLevels.ContainsKey(new LevelId<IslandTypeId>(Island, Level + 1));
		}

		protected override void OnLevelUp(RewardModel rewards, IPlayerModelClientListener listener) {
			listener.OnBuildingLevelUp(Island, rewards);
		}

		protected override void OnXpAdded(
			int delta,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		) {
			listener.OnBuildingXpAdded(Island, delta);
			serverListener.OnBuildingXpAdded(Island, delta);
		}
	}

	[MetaSerializable]
	public class HeroLevelModel : LevelModel {
		[MetaMember(10)]
		public HeroTypeId Hero { get; private set; }

		public HeroLevelModel() { }

		public HeroLevelModel(HeroTypeId hero) {
			Hero = hero;
		}

		protected override RewardMetadata Metadata =>
			new RewardMetadata {
				Type = RewardType.HeroLevel,
				Level = Level,
				Hero = Hero
			};

		public override ILevelInfo GetLevelInfo(SharedGameConfig gameConfig) {
			return gameConfig.HeroLevels[new LevelId<HeroTypeId>(Hero, Level)];
		}

		public override bool HasNextLevel(SharedGameConfig gameConfig) {
			return gameConfig.HeroLevels.ContainsKey(new LevelId<HeroTypeId>(Hero, Level + 1));
		}

		protected override void OnLevelUp(RewardModel rewards, IPlayerModelClientListener listener) {
			listener.OnHeroLevelUp(Hero, rewards);
		}

		protected override void OnXpAdded(
			int delta,
			IPlayerModelClientListener listener,
			IPlayerModelServerListener serverListener,
			ResourceModificationContext context
		) {
			listener.OnHeroXpAdded(Hero, delta);
			serverListener.OnHeroXpAdded(Hero, delta);
		}
	}
}
