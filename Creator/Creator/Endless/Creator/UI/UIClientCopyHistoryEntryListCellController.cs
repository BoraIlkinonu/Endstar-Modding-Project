using System;
using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000E4 RID: 228
	public class UIClientCopyHistoryEntryListCellController : UIBaseListCellController<ClientCopyHistoryEntry>
	{
		// Token: 0x060003D1 RID: 977 RVA: 0x00018814 File Offset: 0x00016A14
		protected override void Start()
		{
			base.Start();
			this.copyTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<CopyTool>();
			this.selectedToggle.OnChange.AddListener(new UnityAction<bool>(this.SetSelectedCopyIndex));
			this.labelInputField.onSubmit.AddListener(new UnityAction<string>(this.UpdateLabel));
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
		}

		// Token: 0x060003D2 RID: 978 RVA: 0x0001888C File Offset: 0x00016A8C
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x060003D3 RID: 979 RVA: 0x000188AB File Offset: 0x00016AAB
		protected override void Remove()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Remove", Array.Empty<object>());
			}
			base.Remove();
			this.copyTool.RemoveIndex(base.DataIndex);
		}

		// Token: 0x060003D4 RID: 980 RVA: 0x000188DC File Offset: 0x00016ADC
		private void SetSelectedCopyIndex(bool state)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "SetSelectedCopyIndex", new object[] { state });
			}
			if (state)
			{
				this.copyTool.SetSelectedCopyIndex(base.DataIndex);
				return;
			}
			this.selectedToggle.SetIsOn(true, true, true);
		}

		// Token: 0x060003D5 RID: 981 RVA: 0x0001892E File Offset: 0x00016B2E
		private void UpdateLabel(string newValue)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateLabel", new object[] { newValue });
			}
			this.copyTool.UpdateCopyLabel(newValue, base.DataIndex);
		}

		// Token: 0x040003EA RID: 1002
		[Header("UIClientCopyHistoryEntryListCellController")]
		[SerializeField]
		private UIToggle selectedToggle;

		// Token: 0x040003EB RID: 1003
		[SerializeField]
		private UIInputField labelInputField;

		// Token: 0x040003EC RID: 1004
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x040003ED RID: 1005
		private CopyTool copyTool;
	}
}
