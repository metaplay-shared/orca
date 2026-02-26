using Code.UI.Application;
using Code.UI.MergeBase;
using Cysharp.Threading.Tasks;
using Game.Logic;
using JetBrains.Annotations;
using Metaplay.Unity.DefaultIntegration;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Code.Logbook {
	[UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
	public class SelectItemTaskOperationProcessor : TaskOperationProcessorBase<SelectItemOperationInfo> {
		private readonly ApplicationInfo applicationInfo;
		private readonly MergeBoardRoot mergeBoardRoot;

		public SelectItemTaskOperationProcessor(
			ApplicationInfo applicationInfo,
			MergeBoardRoot mergeBoardRoot
		) {
			this.applicationInfo = applicationInfo;
			this.mergeBoardRoot = mergeBoardRoot;
		}

		public override UniTask Process(
			SelectItemOperationInfo operation,
			CancellationToken ct
		) {
			IslandTypeId islandTypeId = applicationInfo.ActiveIsland.Value;
			if (islandTypeId == null) {
				Debug.LogWarning("Can't select item when not in a island");
				return UniTask.CompletedTask;
			}

			MergeBoardModel mergeBoard = MetaplayClient.PlayerModel.Islands[islandTypeId].MergeBoard;
			ItemModel targetItem = mergeBoard.Items.FirstOrDefault(
				item =>
					item.Info.ConfigKey.Equals(operation.Item) &&
					mergeBoard.LockArea.IsFree(item.X, item.Y) &&
					item.CanSelect
			);

			if (targetItem == null) {
				Debug.LogWarning("Couldn't find item to select");
				return UniTask.CompletedTask;
			}

			MergeTile builderTargetMergeTile = mergeBoardRoot.TileAt(targetItem.X, targetItem.Y);
			mergeBoardRoot.Select(builderTargetMergeTile.Item, true);

			return UniTask.CompletedTask;
		}
	}
}
