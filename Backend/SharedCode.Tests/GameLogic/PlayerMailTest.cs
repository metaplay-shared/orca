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
			RewardResource goldReward = new RewardResource(15, CurrencyTypeId.Gold);
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

		[Test]
		public void MailWithLogHouseReward() {
			InitModels(new DateTime(2022, 6, 30, 12, 0, 0));
			RewardItem logHouseReward = new RewardItem(amount: 2, ChainTypeId.FromString("LogHouse"), level: 3);
			List<MetaPlayerRewardBase> rewards = new List<MetaPlayerRewardBase> { logHouseReward };

			PlayerMailItem mailItem = CreateMailItem(
				"Test mail with 2x LogHouse:3",
				"Body of the test mail.\nContains level 3 LogHouses.",
				rewards,
				player.CurrentTime - MetaDuration.FromMinutes(30)
			);
			player.MailInbox.Add(mailItem);

			tm.AssertAction(new PlayerConsumeMail(mailItem.Id));
			Assert.AreEqual(2, board.ItemHolder.Count);
			tm.AssertItem("LogHouse:3", board.ItemHolder[0]);
			tm.AssertItem("LogHouse:3", board.ItemHolder[1]);
		}

		[Test]
		public void MailWithMultipleRewards() {
			InitModels(new DateTime(2022, 6, 30, 12, 0, 0));
			RewardResource goldReward = new RewardResource(17, CurrencyTypeId.Gold);
			RewardResource gemReward = new RewardResource(23, CurrencyTypeId.Gems);
			RewardItem itemReward = new RewardItem(amount: 3, ChainTypeId.FromString("LogHouse"), level: 2);
			List<MetaPlayerRewardBase> rewards = new List<MetaPlayerRewardBase> {
				goldReward,
				gemReward,
				itemReward
			};

			MetaTime createdTime = player.CurrentTime - MetaDuration.FromMinutes(50);
			PlayerMailItem mailItem = CreateMailItem("Test mail a lot of rewards", "Body", rewards, createdTime);
			player.MailInbox.Add(mailItem);

			int goldBefore = player.Wallet.Gold.Value;
			int gemsBefore = player.Wallet.Gems.Value;

			tm.AssertAction(new PlayerConsumeMail(mailItem.Id));
			Assert.AreEqual(goldBefore + 17, player.Wallet.Gold.Value);
			Assert.AreEqual(gemsBefore + 23, player.Wallet.Gems.Value);
			Assert.AreEqual(3, board.ItemHolder.Count);
			tm.AssertItem("LogHouse:2", board.ItemHolder[0]);
			tm.AssertItem("LogHouse:2", board.ItemHolder[1]);
			tm.AssertItem("LogHouse:2", board.ItemHolder[2]);
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
