using System;
using Metaplay.Core.Model;

namespace Game.Logic {
	[MetaSerializable]
	public class ConverterModel {
		[MetaMember(1)] public ConverterInfo Info { get; private set; }
		[MetaMember(2)] public int CurrentIndex { get; private set; }
		[MetaMember(3)] public int CurrentValue { get; private set; }

		public bool HasItem => CurrentIndex > 0;

		public ConverterModel() { }

		public ConverterModel(ConverterInfo info) {
			Info = info;
		}

		public bool CanConvertItem(ItemModel item) {
			return item.Info.ConvertableValue > 0;
		}

		public void UseCurrentItem(CreatorModel creator) {
			if (CurrentIndex > 0) {
				// Make sure we don't overflow in case of config changes.
				ItemCountInfo currentItem = Info.Items[Math.Min(CurrentIndex - 1, Info.Items.Count - 1)];
				creator?.ItemQueue.Add(new LevelId<ChainTypeId>(currentItem.Type, currentItem.Level));
				CurrentIndex = 0;
				int value = CurrentValue;
				CurrentValue = 0;
				ConvertValue(creator, value);
			}
		}

		public void ConvertItem(ItemModel item, CreatorModel creator) {
			int value = item.Info.ConvertableValue;
			ConvertValue(creator, value);
		}

		private void ConvertValue(CreatorModel creator, int value) {
			// Make sure we don't overflow in case of config changes.
			ItemCountInfo currentItem = Info.Items[Math.Min(CurrentIndex, Info.Items.Count - 1)];
			while (value > 0) {
				int missing = currentItem.Count - CurrentValue;
				int used = Math.Min(value, missing);
				CurrentValue += used;
				value -= used;
				if (CurrentValue >= currentItem.Count) {
					CurrentIndex++;
					CurrentValue = 0;
					if (CurrentIndex >= Info.Items.Count) {
						creator?.ItemQueue.Add(new LevelId<ChainTypeId>(currentItem.Type, currentItem.Level));
						CurrentIndex = 0;
					}

					currentItem = Info.Items[CurrentIndex];
				}
			}
		}
	}
}
