using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001D3 RID: 467
	public class UIScriptDataTypeSelectionModalView : UIScriptModalView
	{
		// Token: 0x06000700 RID: 1792 RVA: 0x0001FDD0 File Offset: 0x0001DFD0
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x06000701 RID: 1793 RVA: 0x00023679 File Offset: 0x00021879
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			this.scriptDataTypeRadio.SetValue(this.scriptDataTypeRadioDefaultValue, true);
			this.ViewScriptDataTypeInfo(this.scriptDataTypeRadioDefaultValue);
		}

		// Token: 0x06000702 RID: 1794 RVA: 0x000236A0 File Offset: 0x000218A0
		public void ViewScriptDataTypeInfo(ScriptDataTypes scriptDataType)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewScriptDataTypeInfo", new object[] { scriptDataType });
			}
			DisplayNameAndDescription displayNameAndDescription = this.scriptDataTypeInfoDictionary[scriptDataType];
			this.displayNameText.text = displayNameAndDescription.DisplayName;
			this.descriptionText.text = displayNameAndDescription.Description;
		}

		// Token: 0x0400064E RID: 1614
		[Header("UIScriptDataTypeSelectionModalView")]
		[SerializeField]
		private ScriptDataTypeInfoDictionary scriptDataTypeInfoDictionary;

		// Token: 0x0400064F RID: 1615
		[SerializeField]
		private UIScriptDataTypeRadio scriptDataTypeRadio;

		// Token: 0x04000650 RID: 1616
		[SerializeField]
		private ScriptDataTypes scriptDataTypeRadioDefaultValue;

		// Token: 0x04000651 RID: 1617
		[SerializeField]
		private TextMeshProUGUI displayNameText;

		// Token: 0x04000652 RID: 1618
		[SerializeField]
		private TextMeshProUGUI descriptionText;
	}
}
