using System.Linq;
using Code.UI.Application;
using Cysharp.Threading.Tasks;
using Game.Logic;
using Metaplay.Unity.DefaultIntegration;
using UnityEngine;
using Zenject;

namespace Code.UI.Tutorial.TriggerActions {
	public class MergeHintTriggerAction : TriggerAction {
		[Inject] private ApplicationInfo applicationInfo;
		[Inject] private Blackout blackout;

		private ChainTypeId type1;
		private ChainTypeId type2;
		private int level1;
		private int level2;

		public MergeHintTriggerAction(ChainTypeId type1, ChainTypeId type2, int level1, int level2) {
			this.type1 = type1;
			this.type2 = type2;
			this.level1 = level1;
			this.level2 = level2;
		}

		public override async UniTask Run() {
			var itemModelA = FindItem(type1, level1, true, null);
			var itemModelB = FindItem(type2, level2, false, itemModelA);

			//signalBus.Fire(new MergeHintSignal(itemModelA, itemModelB));
			await blackout.MergeHint(itemModelA, itemModelB);

			ItemModel FindItem(ChainTypeId type, int level, bool mustbeFree, ItemModel firstItem) {
				var item = MetaplayClient
					.PlayerModel
					.Islands[applicationInfo.ActiveIsland.Value]
					.MergeBoard
					.Items.FirstOrDefault(
						i => i.Info.Type == type1 &&
							i.Info.Level == level1 &&
							i != firstItem &&
							(i.State == ItemState.Free || !mustbeFree && i.State == ItemState.FreeForMerge)
					);

				if (item == null) {
					Debug.LogWarning($"Item {type} {level} not found on board on active island");
				}

				return item;
			}
		}
	}
}
