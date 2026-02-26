// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using System;
using System.Collections.Generic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Math;
using Metaplay.Core.Player;

namespace Game.Logic {
	public class CustomConfigParsers : ConfigParserProvider {

		public override void RegisterParsers(ConfigParser parser) {
			parser.RegisterCustomParseFunc<ResourceInfo>(ParseResource);
			parser.RegisterCustomParseFunc<LevelId<ChainTypeId>>(ParseItem);
			parser.RegisterCustomParseFunc<Spawnable>(ParseSpawnable);
			parser.RegisterCustomParseFunc<CostRangeInfo>(ParseCostRange);
			parser.RegisterCustomParseFunc<ItemCountInfo>(ParseItemCount);
			parser.RegisterCustomParseFunc<BoardPatternInfo>(ParseBoardPattern);
			parser.RegisterCustomParseFunc<LevelId<IslanderId>>(ParseIslandTask);
			parser.RegisterCustomParseFunc<DialogueWrapperInfo>(ParseDialogueEntries);
			parser.RegisterCustomParseFunc<ColorInfo>(ParseColorInfo);
			parser.RegisterCustomParseFunc<DailyTaskInfo>(ParseDailyTaskInfo);
			parser.RegisterCustomParseFunc<LockAreaUnlockInfo>(ParseLockAreaUnlockInfo);
			parser.RegisterCustomParseFunc<LogbookTaskOperationInfo>(ParseLogbookTaskOperationInfo);
			parser.RegisterCustomParseFunc<PlayerPropertyId>(ParsePlayerPropertyId);
			parser.RegisterCustomParseFunc<PlayerReward>(ParsePlayerReward);
		}

		static object ParseResource(ConfigLexer lexer) {
			int amount = lexer.ParseIntegerLiteral();
			string type = lexer.ParseIdentifier();
			return new ResourceInfo(CurrencyTypeId.FromString(type), amount);
		}

		static object ParseItem(ConfigLexer lexer) {
			string type = lexer.ParseIdentifier();
			int level = lexer.ParseIntegerLiteral();
			return new LevelId<ChainTypeId>(ChainTypeId.FromString(type), level);
		}

		static object ParseSpawnable(ConfigLexer lexer) {
			string type = lexer.ParseIdentifier();
			int level = lexer.ParseIntegerLiteral();
			F64 dropRate = F64.FromDouble(lexer.ParseDoubleLiteral());
			return new Spawnable(ChainTypeId.FromString(type), level, dropRate);
		}

		static object ParseCostRange(ConfigLexer lexer) {
			int rangeEnd = lexer.ParseIntegerLiteral();
			int costPerUnit = lexer.ParseIntegerLiteral();
			return new CostRangeInfo(rangeEnd, costPerUnit);
		}

		static object ParseItemCount(ConfigLexer lexer) {
			string type = lexer.ParseIdentifier();
			int level = lexer.ParseIntegerLiteral();
			int count = lexer.ParseIntegerLiteral();
			return new ItemCountInfo(ChainTypeId.FromString(type), level, count);
		}

		static object ParseBoardPattern(ConfigLexer lexer) {
			string pattern = lexer.ParseStringLiteral();
			BoardPatternInfo patternInfo = new BoardPatternInfo();
			patternInfo.Pattern = pattern;
			return patternInfo;
		}

		static object ParseIslandTask(ConfigLexer lexer) {
			string islander = lexer.ParseIdentifier();
			int id = lexer.ParseIntegerLiteral();
			return new LevelId<IslanderId>(IslanderId.FromString(islander), id);
		}

		static object ParseDialogueEntries(ConfigLexer lexer) {
			List<DialogueEntryInfo> entries = DialogueParser.ParseDialogue(lexer.Input);
			while (!lexer.IsAtEnd) {
				lexer.Advance();
			}

			return new DialogueWrapperInfo(entries);
		}

		static object ParseColorInfo(ConfigLexer lexer) {
			float r = lexer.ParseFloatLiteral();
			float g = lexer.ParseFloatLiteral();
			float b = lexer.ParseFloatLiteral();
			float a = lexer.ParseFloatLiteral();
			return new ColorInfo(r, g, b, a);
		}

		static object ParseDailyTaskInfo(ConfigLexer lexer) {
			string dailyTaskType = lexer.ParseIdentifier();
			int amount = lexer.ParseIntegerLiteral();
			int currencyAmount = lexer.ParseIntegerLiteral();
			string currencyType = lexer.ParseIdentifier();
			string icon = lexer.ParseStringLiteral();
			return new DailyTaskInfo(
				DailyTaskTypeId.FromString(dailyTaskType),
				amount,
				CurrencyTypeId.FromString(currencyType),
				currencyAmount,
				icon
			);
		}

		static object ParseLockAreaUnlockInfo(ConfigLexer lexer) {
			string island = lexer.ParseIdentifier();
			string lockAreaIndex = lexer.GetRemainingInputInfo();
			lexer.Advance();
			return new LockAreaUnlockInfo(IslandTypeId.FromString(island), lockAreaIndex[0]);
		}

		private object ParseLogbookTaskOperationInfo(ConfigLexer lexer) {
			LogbookTaskOperationType type = LogbookTaskOperationType.FromString(lexer.ParseIdentifier());
			if (type == LogbookTaskOperationType.OpenIsland) {
				return new OpenIslandOperationInfo(IslandTypeId.FromString(lexer.ParseIdentifier()));
			}
			if (type == LogbookTaskOperationType.FocusIsland) {
				return new FocusIslandOperationInfo(IslandTypeId.FromString(lexer.ParseIdentifier()));
			}
			if (type == LogbookTaskOperationType.SelectItem) {
				return new SelectItemOperationInfo(
					new LevelId<ChainTypeId>(
						ChainTypeId.FromString(lexer.ParseIdentifier()),
						lexer.ParseIntegerLiteral()
					)
				);
			}
			if (type == LogbookTaskOperationType.OpenDailyTasks) {
				return new OpenDailyTasksOperationInfo();
			}

			throw new Exception($"Invalid logbook task operation type '{type}'");
		}

		static PlayerReward ParsePlayerReward(ConfigLexer lexer)
		{
			int     amount      = lexer.ParseIntegerLiteral();
			string  rewardType  = lexer.ParseIdentifier();

			switch (rewardType)
			{
				case "Gems":
					return new RewardCurrency(CurrencyTypeId.Gems, amount);

				case "Gold":
					return new RewardCurrency(CurrencyTypeId.Gold, amount);

				case "Currency":
				{
					lexer.ParseToken(ConfigLexer.TokenType.ForwardSlash);
					CurrencyTypeId producerTypeId = ConfigParser.Instance.Parse<CurrencyTypeId>(lexer);
					return new RewardCurrency(producerTypeId, amount);
				}
				
				case "Chain":
				{
					lexer.ParseToken(ConfigLexer.TokenType.ForwardSlash);
					LevelId<ChainTypeId> chainTypeId = ConfigParser.Instance.Parse<LevelId<ChainTypeId>>(lexer);
					return new PlayerRewardItem(chainTypeId, amount);
				}

				default:
					throw new ParseError($"Unhandled PlayerReward type in config: {rewardType}");
			}
		}

		static PlayerPropertyId ParsePlayerPropertyId(ConfigLexer lexer)
		{
			if (ConfigParser.TryParseCorePlayerPropertyId(lexer, out PlayerPropertyId propertyId))
				return propertyId;

			string type = lexer.ParseIdentifier();

			switch (type)
			{
				case "GemsPurchased":
					return new PlayerPropertyIdGemsPurchased();

				case "GoldPurchased":
					return new PlayerPropertyIdGoldPurchased();

				case "Island":
				{
					lexer.ParseToken(ConfigLexer.TokenType.ForwardSlash);
					MetaRef<IslandInfo> island = ConfigParser.Instance.Parse<MetaRef<IslandInfo>>(lexer);

					return new PlayerPropertyIdIslandOpen(island);
				}

				case "LastKnownCountry":
					return new PlayerPropertyLastKnownCountry();

				case "AccountCreatedAt":
					return new PlayerPropertyAccountCreatedAt();

				case "AccountAge":
					return new PlayerPropertyAccountAge();

				case "TimeSinceLastLogin":
					return new PlayerPropertyTimeSinceLastLogin();
			}

			throw new ParseError($"Invalid PlayerPropertyId in config: {type}");
		}
	}
}
