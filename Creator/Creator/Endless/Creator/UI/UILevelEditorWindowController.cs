using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002D8 RID: 728
	public class UILevelEditorWindowController : UIWindowController
	{
		// Token: 0x06000C5A RID: 3162 RVA: 0x0003B248 File Offset: 0x00039448
		protected override void Start()
		{
			base.Start();
			this.BaseWindowView.CloseUnityEvent.AddListener(new UnityAction(this.OnClosing));
			this.spawnPointListModel.ItemSelectedUnityEvent.AddListener(new UnityAction<int>(this.OnSpawnPointSelected));
			this.spawnPointListModel.ItemUnselectedUnityEvent.AddListener(new UnityAction<int>(this.OnSpawnPointUnselected));
			this.spawnPointListModel.ItemSwappedUnityEvent.AddListener(new UnityAction<int, int>(this.OnItemSwappedUnityEvent));
		}

		// Token: 0x06000C5B RID: 3163 RVA: 0x0003A119 File Offset: 0x00038319
		public override void Close()
		{
			base.Close();
			MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
		}

		// Token: 0x06000C5C RID: 3164 RVA: 0x0003B2CC File Offset: 0x000394CC
		private void OnSpawnPointsChanged()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawnPointsChanged", Array.Empty<object>());
			}
			SerializableGuid[] array = this.spawnPointListModel.SpawnPointIds.ToArray();
			SerializableGuid[] array2 = this.spawnPointListModel.SelectedSpawnPointIds.ToArray();
			NetworkBehaviourSingleton<CreatorManager>.Instance.LevelEditor.UpdateLevelSpawnPointOrder_ServerRpc(array, array2, default(ServerRpcParams));
		}

		// Token: 0x06000C5D RID: 3165 RVA: 0x0003B32D File Offset: 0x0003952D
		private void OnClosing()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnClosing", Array.Empty<object>());
			}
			if (MonoBehaviourSingleton<ToolManager>.Instance.ActiveTool.ToolType == ToolType.LevelEditor)
			{
				MonoBehaviourSingleton<ToolManager>.Instance.SetActiveTool(ToolType.Empty);
			}
		}

		// Token: 0x06000C5E RID: 3166 RVA: 0x0003B365 File Offset: 0x00039565
		private void OnSpawnPointSelected(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawnPointSelected", new object[] { index });
			}
			this.OnSpawnPointsChanged();
		}

		// Token: 0x06000C5F RID: 3167 RVA: 0x0003B38F File Offset: 0x0003958F
		private void OnSpawnPointUnselected(int index)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnSpawnPointUnselected", new object[] { index });
			}
			this.OnSpawnPointsChanged();
		}

		// Token: 0x06000C60 RID: 3168 RVA: 0x0003B3B9 File Offset: 0x000395B9
		private void OnItemSwappedUnityEvent(int indexA, int indexB)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnItemSwappedUnityEvent", new object[] { indexA, indexB });
			}
			this.OnSpawnPointsChanged();
		}

		// Token: 0x04000AA7 RID: 2727
		[Header("UILevelEditorWindowController")]
		[SerializeField]
		private UIScreenshotFileInstancesListModel screenshotFileInstancesListModel;

		// Token: 0x04000AA8 RID: 2728
		[SerializeField]
		private UISpawnPointListModel spawnPointListModel;
	}
}
