using System;
using System.Collections.Generic;
using CloudCore.Tests.GameLogic.Utils;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.InGameMail;
using Metaplay.Core.Localization;
using Metaplay.Core.Player;
using Metaplay.Core.Rewards;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class PlayerMailTest {
		private TestModel tm;
		private PlayerModel player;
		private MergeBoardModel board;

		private void InitModels(DateTime time) {
			tm = CommonUtils.CreateTestModel(MetaTime.FromDateTime(time));
			player = tm.PlayerModel;
			board = player.Islands[IslandTypeId.MainIsland].MergeBoard;

			tm.BoardDeleteAllItems(board);
			// Fill the item holder tiles to simplify asserting the consumed rewards.
			board.CreateItem(0, 0, tm.CreateItem("Orange:1"));
			board.CreateItem(1, 0, tm.CreateItem("Orange:1"));
		}

		[Test]
		public void MailWithGoldReward() {
			InitModels(new DateTime(2022, 6, 30, 12, 0, 0));
			RewardCurrency goldReward = new RewardCurrency(CurrencyTypeId.Gold, 15);
			List<MetaPlayerRewardBase> rewards = new List<MetaPlayerRewardBase> { goldReward };

			PlayerMailItem mailItem = CreateMailItem(
				"Test mail with 15 gold",
				"Body of the test mail.\nContains 15 gold",
				rewards,
				player.CurrentTime - MetaDuration.FromMinutes(50)
			);
			player.MailInbox.Add(mailItem);

			int goldBefore = player.Wallet.Gold.Value;
			tm.AssertAction(new PlayerConsumeMail(mailItem.Id));
			Assert.AreEqual(goldBefore + 15, player.Wallet.Gold.Value);
		}

		private PlayerMailItem CreateMailItem(
			string title,
			string body,
			List<MetaPlayerRewardBase> rewards,
			MetaTime created
		) {
			SimplePlayerMail contents = new SimplePlayerMail(
				LanguageId.FromString("en"),
				title,
				body,
				rewards,
				MetaGuid.NewWithTime(created.ToDateTime())
			);
			return new DefaultPlayerMailItem(contents, created - MetaDuration.FromMinutes(1));
		}
	}
}
