using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000AF RID: 175
	public class UILevelController : UIAssetWithScreenshotsController
	{
		// Token: 0x060002BC RID: 700 RVA: 0x00012468 File Offset: 0x00010668
		protected override void Start()
		{
			base.Start();
			this.screenshotTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<ScreenshotTool>();
			this.screenshotFileInstancesListModel.OnItemMovedUnityEvent.AddListener(new UnityAction<int, int>(this.RearrangeScreenshots));
			this.removeScreenshotDropHandler.DropWithGameObjectUnityEvent.AddListener(new UnityAction<GameObject>(this.OnDroppedScreenshot));
		}

		// Token: 0x060002BD RID: 701 RVA: 0x000124C4 File Offset: 0x000106C4
		protected override void SetName(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetName", new object[] { newValue });
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Name == newValue)
			{
				return;
			}
			if (newValue.Replace(" ", string.Empty).IsNullOrEmptyOrWhiteSpace())
			{
				base.NameInputField.PlayInvalidInputTweens();
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.UpdateLevelName_ServerRpc(newValue, default(ServerRpcParams));
		}

		// Token: 0x060002BE RID: 702 RVA: 0x00012548 File Offset: 0x00010748
		protected override void SetDescription(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetDescription", new object[] { newValue });
			}
			if (MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.LevelState.Description == newValue)
			{
				return;
			}
			NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.UpdateLevelDescription_ServerRpc(newValue, default(ServerRpcParams));
		}

		// Token: 0x060002BF RID: 703 RVA: 0x000125A8 File Offset: 0x000107A8
		protected override void RemoveScreenshot(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveScreenshot", new object[] { index });
			}
			DebugUtility.LogMethod(this, "RemoveScreenshot", new object[] { index });
			this.screenshotTool.RemoveScreenshotFromLevel_ServerRPC(index, default(ServerRpcParams));
			bool flag = index > 0;
			this.screenshotFileInstancesListModel.RemoveAt(index, flag);
			if (index == 0)
			{
				this.levelView.DisplayScreenshots(this.screenshotFileInstancesListModel.ReadOnlyList.ToList<ScreenshotFileInstances>());
			}
		}

		// Token: 0x060002C0 RID: 704 RVA: 0x00012638 File Offset: 0x00010838
		protected override void RearrangeScreenshots(int oldIndex, int newIndex)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RearrangeScreenshots", new object[] { oldIndex, newIndex });
			}
			List<ScreenshotFileInstances> list = this.screenshotFileInstancesListModel.ReadOnlyList.ToList<ScreenshotFileInstances>();
			this.screenshotTool.RearrangeScreenshotsToLevel_ServerRPC(list.ToArray(), default(ServerRpcParams));
			this.levelView.DisplayScreenshots(list);
		}

		// Token: 0x060002C1 RID: 705 RVA: 0x000126A8 File Offset: 0x000108A8
		private void OnDroppedScreenshot(GameObject droppedGameObject)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDroppedScreenshot", new object[] { droppedGameObject.DebugSafeName(true) });
			}
			UIScreenshotFileInstancesListCellView uiscreenshotFileInstancesListCellView;
			if (!droppedGameObject.TryGetComponent<UIScreenshotFileInstancesListCellView>(out uiscreenshotFileInstancesListCellView))
			{
				throw new NullReferenceException("Could not get UIScreenshotFileInstancesListCellView from droppedGameObject!");
			}
			int num = this.screenshotFileInstancesListModel.ReadOnlyList.IndexOf(uiscreenshotFileInstancesListCellView.Model);
			if (num < 0)
			{
				throw new IndexOutOfRangeException("Could not get index of " + uiscreenshotFileInstancesListCellView.Model.GetType().Namespace + " from screenshotFileInstancesListModel!");
			}
			this.RemoveScreenshot(num);
		}

		// Token: 0x040002F9 RID: 761
		[Header("UILevelController")]
		[SerializeField]
		private UILevelView levelView;

		// Token: 0x040002FA RID: 762
		[SerializeField]
		private UIScreenshotView mainScreenshot;

		// Token: 0x040002FB RID: 763
		[SerializeField]
		private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

		// Token: 0x040002FC RID: 764
		[SerializeField]
		private UIDropHandler removeScreenshotDropHandler;

		// Token: 0x040002FD RID: 765
		private ScreenshotTool screenshotTool;
	}
}
