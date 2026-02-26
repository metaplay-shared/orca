using System.Collections.Generic;
using Game.Logic;
using NUnit.Framework;
using System.IO;
using System.Linq;
using CloudCore.Tests.GameLogic.Utils;

namespace CloudCore.Tests.GameLogic {
	[TestFixture]
	public class AssetTest {
		[Test]
		public void ChainGraphicsExists() {
			SharedGameConfig config = CommonUtils.LoadGameConfig(false);

			List<string> missingAssets = new();

			foreach (ChainInfo chain in config.Chains.Values) {
				string assetBaseDir = $"{CommonUtils.OrcaProjectRootDir}/Assets/Graphics/Chains";
				string assetPath = $"{assetBaseDir}/{chain.Type}/{chain.Type}{chain.Level}.png";
				var exists = File.Exists(assetPath);

				if (!exists) {
					missingAssets.Add($"{chain.Type}{chain.Level}.png");
				}
			}

			Assert.Zero(
				missingAssets.Count,
				$"Following items do not have asset: {missingAssets.Aggregate("", (current, next) => current + ", " + next)}"
			);
		}
	}
}
