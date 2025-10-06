using System.IO;
using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Config;
using Metaplay.Core.Player;
using NUnit.Framework;

namespace CloudCore.Tests.GameLogic.Utils {
	public static class CommonUtils {
		public static readonly string OrcaProjectRootDir;

		static CommonUtils() {
			// Traverse up in the directory tree to find the project root directory. This way we don't need to specify
			// relative paths that are different depending on how the tests are run (from IDE or command line).
			string dir = Directory.GetCurrentDirectory();
			do {
				if (Directory.Exists($"{dir}/Backend")) {
					OrcaProjectRootDir = dir;
					return;
				}

				dir = Directory.GetParent(dir)?.FullName;
			} while (dir != null);

			Assert.Fail("Unable to set project root dir");
		}

		public static SharedGameConfig LoadGameConfig(bool useUnitTestConfig = true) {
            // Load config
            string configPath = useUnitTestConfig
				? $"{OrcaProjectRootDir}/Backend/SharedCode.Tests/resources/UnitTesting-SharedGameConfig.mpa"
				: $"{OrcaProjectRootDir}/Assets/StreamingAssets/SharedGameConfig.mpa";
			ConfigArchive configArchive = ConfigArchive.FromFile(configPath);
			SharedGameConfig gameConfig = (SharedGameConfig)GameConfigUtil.ImportSharedConfig(configArchive);
			return gameConfig;
		}

		public static TestModel CreateTestModel(MetaTime startTime, SharedGameConfig gameConfig = null) {
			gameConfig ??= LoadGameConfig();

			PlayerModel playerModel = CreatePlayerModel(startTime, gameConfig);
			return new TestModel(playerModel);
		}

		public static TestModel CreateTestModel(SharedGameConfig gameConfig = null) {
			gameConfig ??= LoadGameConfig();

			PlayerModel playerModel = CreatePlayerModel(MetaTime.Epoch, gameConfig);
			return new TestModel(playerModel);
		}

		public static PlayerModel CreatePlayerModel(MetaTime startTime, SharedGameConfig gameConfig = null) {
			PlayerModel playerModel = PlayerModelUtil.CreateNewPlayerModel<PlayerModel>(
				startTime,
				gameConfig,
				playerId: EntityId.None,
				name: null
			);
			playerModel.LogicVersion = 1;

			Assert.AreEqual(startTime, playerModel.CurrentTime);

			return playerModel;
		}

        public static void FastForwardTime(PlayerModel player, MetaDuration elapsed) {
            MetaTime targetTime = player.CurrentTime + elapsed;
            player.ResetTime(targetTime);
            player.OnFastForwardTime(elapsed);
            MetaDebug.Assert(player.CurrentTime == targetTime, "FastForwardTime didn't reach the target time exactly");
        }
	}
}
