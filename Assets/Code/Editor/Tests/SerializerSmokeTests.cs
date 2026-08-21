using Game.Logic;
using Metaplay.Core;
using Metaplay.Core.Serialization;
using Metaplay.Core.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orca.Editor.Tests {
	/// <summary>
	/// Smoke tests for Metaplay's serializer generation against the game's own types.
	///
	/// The tagged serializer is generated and then compiled by Unity's own compiler API
	/// (UnityAssemblyBuilder, via RoslynSerializerCompileCache), which makes it one of the
	/// parts of this project most sensitive to a Unity version change. In normal Editor use
	/// that generation runs from an [InitializeOnLoadMethod] which only logs failures
	/// (MetaplaySDK/Client/Unity/Editor/EditorInit.cs), so a break is easy to miss. These
	/// tests turn it into a failing test, and check that the result covers the game's types
	/// rather than only the SDK's.
	///
	/// Run with: uv run tools/orca.py client test  (or `unity test --mode EditMode`)
	/// </summary>
	public class SerializerSmokeTests {
		private IMetaplayServiceProvider previousProvider;

		[OneTimeSetUp]
		public void SetUp() {
			// Regenerates and compiles the tagged serializer for every scanned type,
			// the game's included. Throws if generation or compilation fails.
			previousProvider = TestHelper.SetupForTests();
		}

		[OneTimeTearDown]
		public void TearDown() {
			// Null if SetUp threw.
			if (previousProvider != null) {
				MetaplayServices.SetServiceProvider(previousProvider);
			}
		}

		[Test]
		public void GeneratedSerializerCoversGameTypes() {
			List<Type> gameTypes = MetaSerializerTypeRegistry.Instance.AllTypes
				.Select(spec => spec.Type)
				.Where(type => type.Namespace != null && type.Namespace.StartsWith("Game.Logic", StringComparison.Ordinal))
				.ToList();

			Assert.That(gameTypes, Is.Not.Empty, "the generated serializer covers no Game.Logic types at all");
			Assert.That(gameTypes, Contains.Item(typeof(PlayerModel)), "PlayerModel is missing from the generated serializer");
		}

		[Test]
		public void GameTypeRoundTripsThroughGeneratedSerializer() {
			// IslandCoordinate is a StringId plus two ints, so this round-trip needs no
			// game config resolver - it exercises generated code, not config loading.
			IslandCoordinate original = new IslandCoordinate(IslandTypeId.MainIsland, 3, -4);

			byte[] serialized = MetaSerialization.SerializeTagged(original, MetaSerializationFlags.IncludeAll, logicVersion: null);
			IslandCoordinate restored = MetaSerialization.DeserializeTagged<IslandCoordinate>(
				serialized,
				MetaSerializationFlags.IncludeAll,
				resolver: null,
				logicVersion: null
			);

			Assert.That(restored, Is.EqualTo(original));
		}
	}
}
