using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020000E9 RID: 233
	public class UIConsoleMessageListController : UIBaseLocalFilterableListController<UIConsoleMessageModel>
	{
		// Token: 0x060003E4 RID: 996 RVA: 0x00018CC0 File Offset: 0x00016EC0
		protected override void Start()
		{
			base.Start();
			this.showInfo.OnChange.AddListener(new UnityAction<bool>(this.OnToggleChange));
			this.showWarning.OnChange.AddListener(new UnityAction<bool>(this.OnToggleChange));
			this.showError.OnChange.AddListener(new UnityAction<bool>(this.OnToggleChange));
			this.LocalFilterableListModel.ModelChangedUnityEvent.AddListener(new UnityAction(this.OnModelChange));
			this.OnModelChange();
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x00018D4C File Offset: 0x00016F4C
		private void OnModelChange()
		{
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			IReadOnlyList<UIConsoleMessageModel> readOnlyList = this.LocalFilterableListModel.ReadOnlyList;
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				switch (readOnlyList[i].Message.LogType)
				{
				case LogType.Error:
				case LogType.Assert:
				case LogType.Exception:
					num3++;
					break;
				case LogType.Warning:
					num2++;
					break;
				case LogType.Log:
					num++;
					break;
				default:
					Debug.LogError(string.Format("Unhandled Message LogType: {0}", this.LocalFilterableListModel[i].Message.LogType));
					break;
				}
			}
			this.infoToggleLabel.text = ((num == 0) ? string.Empty : num.ToString());
			this.warningToggleLabel.text = ((num2 == 0) ? string.Empty : num2.ToString());
			this.errorToggleLabel.text = ((num3 == 0) ? string.Empty : num3.ToString());
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x00018E42 File Offset: 0x00017042
		private void OnToggleChange(bool newValue)
		{
			this.LocalFilterableListModel.Filter(new Func<UIConsoleMessageModel, bool>(this.IncludeInFilteredResults), true);
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x00018E60 File Offset: 0x00017060
		protected override bool IncludeInFilteredResults(UIConsoleMessageModel item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
			}
			if (item == null)
			{
				DebugUtility.LogError("PropLibrary.RuntimePropInfo was null!", this);
				return false;
			}
			string text = item.Message.Message;
			if (!base.CaseSensitive)
			{
				text = text.ToLower();
			}
			string text2 = base.StringFilter;
			if (!base.CaseSensitive)
			{
				text2 = text2.ToLower();
			}
			switch (item.Message.LogType)
			{
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				if (!this.showError.IsOn)
				{
					return false;
				}
				break;
			case LogType.Warning:
				if (!this.showWarning.IsOn)
				{
					return false;
				}
				break;
			case LogType.Log:
				if (!this.showInfo.IsOn)
				{
					return false;
				}
				break;
			default:
				Debug.LogError(string.Format("Unhandled Message LogType: {0}", item.Message.LogType));
				break;
			}
			return text.Contains(text2);
		}

		// Token: 0x040003FF RID: 1023
		[SerializeField]
		private UIToggle showInfo;

		// Token: 0x04000400 RID: 1024
		[SerializeField]
		private TextMeshProUGUI infoToggleLabel;

		// Token: 0x04000401 RID: 1025
		[SerializeField]
		private UIToggle showWarning;

		// Token: 0x04000402 RID: 1026
		[SerializeField]
		private TextMeshProUGUI warningToggleLabel;

		// Token: 0x04000403 RID: 1027
		[SerializeField]
		private UIToggle showError;

		// Token: 0x04000404 RID: 1028
		[SerializeField]
		private TextMeshProUGUI errorToggleLabel;
	}
}
