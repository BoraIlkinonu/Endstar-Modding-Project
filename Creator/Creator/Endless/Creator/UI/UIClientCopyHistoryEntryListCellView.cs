using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Gameplay.LevelEditing;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000E5 RID: 229
	public class UIClientCopyHistoryEntryListCellView : UIBaseListCellView<ClientCopyHistoryEntry>
	{
		// Token: 0x060003D7 RID: 983 RVA: 0x00018968 File Offset: 0x00016B68
		public override void OnSpawn()
		{
			base.OnSpawn();
			if (!this.copyTool)
			{
				this.copyTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<CopyTool>();
			}
			this.copyTool.CopyHistoryEntryInserted.AddListener(new UnityAction<ClientCopyHistoryEntry>(this.CopyHistoryEntryInserted));
			this.copyTool.CopyHistoryTrimmed.AddListener(new UnityAction<int>(this.OnCopyHistoryTrimmed));
		}

		// Token: 0x060003D8 RID: 984 RVA: 0x000189D0 File Offset: 0x00016BD0
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.copyTool.CopyHistoryEntryInserted.RemoveListener(new UnityAction<ClientCopyHistoryEntry>(this.CopyHistoryEntryInserted));
			this.copyTool.CopyHistoryTrimmed.RemoveListener(new UnityAction<int>(this.OnCopyHistoryTrimmed));
		}

		// Token: 0x060003D9 RID: 985 RVA: 0x00018A10 File Offset: 0x00016C10
		public override void View(UIBaseListView<ClientCopyHistoryEntry> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			PropLibrary.RuntimePropInfo runtimePropInfo = MonoBehaviourSingleton<StageManager>.Instance.ActivePropLibrary.GetRuntimePropInfo(base.Model.AssetId);
			this.displayIconImage.sprite = runtimePropInfo.Icon;
			this.UpdateSelectedToggle();
			this.labelInputField.text = base.Model.Label;
		}

		// Token: 0x060003DA RID: 986 RVA: 0x00018A70 File Offset: 0x00016C70
		private void UpdateSelectedToggle()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateSelectedToggle", Array.Empty<object>());
			}
			bool flag = this.copyTool.SelectedIndex == base.DataIndex;
			this.selectedToggle.SetIsOn(flag, true, false);
		}

		// Token: 0x060003DB RID: 987 RVA: 0x00018AB7 File Offset: 0x00016CB7
		private void CopyHistoryEntryInserted(ClientCopyHistoryEntry newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CopyHistoryEntryInserted", new object[] { newValue.Label });
			}
			this.UpdateSelectedToggle();
		}

		// Token: 0x060003DC RID: 988 RVA: 0x00018AE1 File Offset: 0x00016CE1
		private void OnCopyHistoryTrimmed(int endTrimCount)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnCopyHistoryTrimmed", new object[] { endTrimCount });
			}
			this.UpdateSelectedToggle();
		}

		// Token: 0x040003EE RID: 1006
		[Header("UIClientCopyHistoryEntryListCellView")]
		[SerializeField]
		private Image displayIconImage;

		// Token: 0x040003EF RID: 1007
		[SerializeField]
		private UIToggle selectedToggle;

		// Token: 0x040003F0 RID: 1008
		[SerializeField]
		private UIInputField labelInputField;

		// Token: 0x040003F1 RID: 1009
		private CopyTool copyTool;
	}
}
