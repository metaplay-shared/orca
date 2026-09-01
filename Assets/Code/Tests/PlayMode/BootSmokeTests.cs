using Game.Logic;
using Metaplay.Core.Client;
using Metaplay.Core;
using Metaplay.Unity.DefaultIntegration;
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Orca.Tests.PlayMode {
	/// <summary>
	/// PlayMode smoke tests: does the client actually start up, reach gameplay, and run the
	/// opening of the tutorial?
	///
	/// Reaching the Game scene requires a full login and session handshake, the game configs
	/// to load, and the Zenject wiring in the Start scene to come up - so a single assertion
	/// on the active scene covers a lot of ground that compiling alone does not.
	///
	/// Runs against offline mode by default, which needs no backend. Point it at a real server
	/// instead by setting the environment id (see <see cref="EnvironmentVariable"/>):
	///
	///   uv run tools/orca.py client test --mode PlayMode --reset
	///   uv run tools/orca.py client test --mode PlayMode --with-server -e localhost --reset
	///
	/// --reset matters: without it the offline server restores this machine's saved player, so
	/// a developer replays existing progress while CI starts from nothing.
	/// </summary>
	public class BootSmokeTests {
		/// <summary>
		/// Set by tools/orca.py. `unity test` has no argument passthrough, unlike `unity build`,
		/// so the environment id has to travel to the editor as an environment variable.
		/// </summary>
		private const string EnvironmentVariable = "ORCA_TEST_ENVIRONMENT";

		private const string DefaultEnvironmentId = "offline";

		/// <summary>
		/// Generous: a cold Play-mode entry compiles, loads configs and runs a handshake.
		/// </summary>
		private const float BootTimeoutSeconds = 120f;

		/// <summary>
		/// The tutorial's opening move. A fresh MainIsland board puts two level-1 HeroRanger
		/// items next to each other, and (5,1) is the only item the board reports as movable,
		/// so merging it onto (4,1) is the first thing a new player can do.
		/// </summary>
		private static readonly ChainTypeId HeroRangerType = ChainTypeId.FromString("HeroRanger");
		private const int FirstRangerX = 4, FirstRangerY = 1;
		private const int SecondRangerX = 5, SecondRangerY = 1;

		/// <summary>
		/// Fires the first time a level-2 HeroRanger is discovered: it is listed in the Chains
		/// config's DiscoveredTriggers for that item, and PlayerModel fires those through
		/// Merge.ItemDiscovery.SetDiscovery, which only reports a discovery once per item ever.
		/// That is the real reason this test needs a fresh player, and a config rebalance rather
		/// than a code change is what would move this id.
		/// </summary>
		private static readonly TriggerId RangerLevel2Trigger = TriggerId.FromString("RangerHeroItemLevel2");

		/// <summary>Frames to let an action round-trip through the server and back.</summary>
		private const int ActionSettleFrames = 30;

		private string _environmentId;

		[OneTimeSetUp]
		public void SetUp() {
			_environmentId = Environment.GetEnvironmentVariable(EnvironmentVariable);
			if (string.IsNullOrEmpty(_environmentId)) {
				_environmentId = DefaultEnvironmentId;
			}

			// Force the environment for the duration of this play session.
			//
			// The offline entry is first in ProjectSettings/Metaplay/EnvironmentConfigs.json and
			// would be chosen by default, but a developer machine can carry an editor override
			// pointing at a real server - and while playing, GetActiveEnvironmentId() consults
			// the runtime override first and then a value cached at editor load, so writing the
			// editor override here would be too late. The runtime override applies for this
			// process only and needs no cleanup.
			DefaultEnvironmentConfigProvider.Instance.SetActiveEnvironmentIdRuntimeOverride(_environmentId);
		}

		[UnityTest]
		public IEnumerator BootsIntoTheGameScene() {
			yield return EnsureBootedIntoTheGame();

			// \note Not the game's `MetaplayClient`: that lives in Assembly-CSharp, which an
			// asmdef assembly cannot reference. Its statics are the closed generic base's, so
			// this reads the very same state the game set up.
			Assert.That(MetaplayClientBase<PlayerModel>.IsInitialized, Is.True, "the Metaplay client is not initialised");
			Assert.That(MetaplayClientBase<PlayerModel>.PlayerModel, Is.Not.Null, "no PlayerModel after session start");

			PlayerModel player = MetaplayClientBase<PlayerModel>.PlayerModel;
			TestContext.WriteLine($"environment: {_environmentId}, island: {player.CurrentIsland}");
		}

		/// <summary>
		/// Drives the first real player action and checks the game reacted to it.
		///
		/// This is the part a compile or a boot cannot cover: the action goes through the shared
		/// game logic on both the client and the server, and with ClientConsistencyChecks enabled
		/// (the default on development environments) a divergence between the two would surface
		/// here rather than in production.
		/// </summary>
		[UnityTest]
		public IEnumerator MergingTheFirstHeroItemsAdvancesTheTutorial() {
			yield return EnsureBootedIntoTheGame();

			PlayerModel player = MetaplayClientBase<PlayerModel>.PlayerModel;
			MergeBoardModel board = player.Islands[IslandTypeId.MainIsland].MergeBoard;

			// Only meaningful on a fresh player: the tiles would otherwise hold whatever this
			// machine's saved game left there, and the discovery trigger would already have
			// fired for good. Skip rather than fail, so a local run without --reset reports the
			// reason instead of a confusing assertion failure.
			if (!IsLevelOneRanger(board, FirstRangerX, FirstRangerY) ||
				!IsLevelOneRanger(board, SecondRangerX, SecondRangerY)) {
				Assert.Ignore(
					$"needs a fresh player: expected level-1 {HeroRangerType} items at " +
					$"({FirstRangerX},{FirstRangerY}) and ({SecondRangerX},{SecondRangerY}). " +
					"Re-run with --reset."
				);
			}

			MetaplayClientBase<PlayerModel>.PlayerContext.ExecuteAction(
				new PlayerMoveItemOnBoard(IslandTypeId.MainIsland, SecondRangerX, SecondRangerY, FirstRangerX, FirstRangerY)
			);
			for (int frame = 0; frame < ActionSettleFrames; frame++) {
				yield return null;
			}

			board = player.Islands[IslandTypeId.MainIsland].MergeBoard;
			ItemModel merged = board[FirstRangerX, FirstRangerY].Item;

			Assert.That(merged, Is.Not.Null, $"nothing left at ({FirstRangerX},{FirstRangerY}) after the merge");
			Assert.That(merged.Info.Type, Is.EqualTo(HeroRangerType), "the merged item is not a hero ranger");
			Assert.That(merged.Info.Level, Is.EqualTo(2), "the two level-1 rangers did not merge into a level-2 one");
			Assert.That(
				board[SecondRangerX, SecondRangerY].HasItem,
				Is.False,
				$"({SecondRangerX},{SecondRangerY}) should be empty once its item merged away"
			);
			Assert.That(
				player.Triggers.Executed.ContainsKey(RangerLevel2Trigger),
				Is.True,
				$"the merge did not fire {RangerLevel2Trigger}; triggers so far: " +
				string.Join(", ", player.Triggers.Executed.Keys)
			);

			TestContext.WriteLine(
				$"tutorial advanced, triggers ({player.Triggers.Executed.Count}): " +
				string.Join(", ", player.Triggers.Executed.Keys)
			);
		}

		private static bool IsLevelOneRanger(MergeBoardModel board, int x, int y) {
			ItemModel item = board[x, y].Item;
			return item != null && item.Info.Type == HeroRangerType && item.Info.Level == 1;
		}

		/// <summary>
		/// Boots the game if this play session has not already reached it, so each test stands on
		/// its own rather than depending on NUnit's ordering.
		/// </summary>
		private IEnumerator EnsureBootedIntoTheGame() {
			if (SceneManager.GetActiveScene().name == "Game") {
				yield break;
			}

			yield return SceneManager.LoadSceneAsync("Start");

			Stopwatch elapsed = Stopwatch.StartNew();
			while (SceneManager.GetActiveScene().name != "Game" && elapsed.Elapsed.TotalSeconds < BootTimeoutSeconds) {
				yield return null;
			}

			Assert.That(
				SceneManager.GetActiveScene().name,
				Is.EqualTo("Game"),
				$"the client did not reach the Game scene within {BootTimeoutSeconds}s using the " +
				$"'{_environmentId}' environment; it stopped at '{SceneManager.GetActiveScene().name}'. " +
				"The Game scene is only loaded after a successful login and session start, so check the " +
				"log for config or handshake failures."
			);
			TestContext.WriteLine($"booted in {elapsed.Elapsed.TotalSeconds:F1}s");
		}
	}
}
