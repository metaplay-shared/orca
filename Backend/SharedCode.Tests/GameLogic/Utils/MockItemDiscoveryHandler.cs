using System;
using System.Collections.Generic;
using Game.Logic;

namespace CloudCore.Tests.GameLogic.Utils {
	public class MockItemDiscoveryHandler {
		public bool DebugOn = false;
		public List<ItemModel> Items { get; private set; } = new();

		public void HandleItemDiscovery(ItemModel item) {
			if (DebugOn) {
				Console.Out.WriteLine($"Item discovered handler: {item}");
			}

			Items.Add(item);
		}
	}
}
