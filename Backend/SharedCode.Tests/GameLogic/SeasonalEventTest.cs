using System;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class SeasonalEventTest {
		private TestModel tm;
		private PlayerModel player;
		private SharedGameConfig config;
		private MergeBoardModel board;
		private IslandTypeId island;

		private void InitModels(DateTime time) {
			tm = CommonUtils.CreateTestModel(MetaTime.FromDateTime(time));
			player = tm.PlayerModel;
			config = tm.GameConfig;
			player.PrivateProfile.FeaturesEnabled.Add(FeatureTypeId.SeasonalEvents);

			island = IslandTypeId.MainIsland;
			board = player.Islands[island].MergeBoard;
			tm.BoardDeleteAllItems(board);
			tm.PrimaryBoard = board;
		}
	}
}
