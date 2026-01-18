using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200029C RID: 668
	public class UICopyToolPanelController : UIGameObject, IBackable
	{
		// Token: 0x06000B1B RID: 2843 RVA: 0x0003421C File Offset: 0x0003241C
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.selectMoreButton.onClick.AddListener(new UnityAction(this.SelectMore));
			this.copyTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<CopyTool>();
			this.copyTool.CopyHistoryEntryInserted.AddListener(new UnityAction<ClientCopyHistoryEntry>(this.CopyHistoryEntryInserted));
			this.copyTool.CopyHistoryTrimmed.AddListener(new UnityAction<int>(this.CopyHistoryTrimmed));
			this.copyTool.CopyHistoryCleared.AddListener(new UnityAction(this.CopyHistoryCleared));
		}

		// Token: 0x06000B1C RID: 2844 RVA: 0x000342C1 File Offset: 0x000324C1
		public void OnBack()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			this.SelectMore();
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
		}

		// Token: 0x06000B1D RID: 2845 RVA: 0x000342FC File Offset: 0x000324FC
		private void SelectMore()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "SelectMore", Array.Empty<object>());
			}
			this.copyTool.SetSelectedCopyIndex(-1);
			this.clientCopyHistoryEntryListModel.ClearSelected(true);
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
		}

		// Token: 0x06000B1E RID: 2846 RVA: 0x00034351 File Offset: 0x00032551
		private void CopyHistoryTrimmed(int endTrimCount)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CopyHistoryTrimmed", new object[] { endTrimCount });
			}
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x06000B1F RID: 2847 RVA: 0x0003438D File Offset: 0x0003258D
		private void CopyHistoryEntryInserted(ClientCopyHistoryEntry clientCopyHistoryEntry)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CopyHistoryEntryInserted", new object[] { clientCopyHistoryEntry });
			}
			if (!MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.ClaimContext(this);
			}
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x000343C4 File Offset: 0x000325C4
		private void CopyHistoryCleared()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "CopyHistoryCleared", Array.Empty<object>());
			}
			if (MonoBehaviourSingleton<BackManager>.Instance.HasContext(this))
			{
				MonoBehaviourSingleton<BackManager>.Instance.UnclaimContext(this);
			}
		}

		// Token: 0x04000966 RID: 2406
		[SerializeField]
		private UIButton selectMoreButton;

		// Token: 0x04000967 RID: 2407
		[SerializeField]
		private UIClientCopyHistoryEntryListModel clientCopyHistoryEntryListModel;

		// Token: 0x04000968 RID: 2408
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000969 RID: 2409
		private CopyTool copyTool;
	}
}
