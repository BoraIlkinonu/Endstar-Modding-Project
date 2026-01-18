using System;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Core.UI
{
	// Token: 0x0200006D RID: 109
	public class UILogInspectorModalView : UIEscapableModalView
	{
		// Token: 0x060001FD RID: 509 RVA: 0x0000B504 File Offset: 0x00009704
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			UILog uilog = (UILog)modalData[0];
			this.logTypeText.text = uilog.Type.ToString();
			this.conditionInputField.text = uilog.Condition;
			this.stackTraceInputField.text = uilog.StackTrace;
		}

		// Token: 0x0400016B RID: 363
		[SerializeField]
		private TextMeshProUGUI logTypeText;

		// Token: 0x0400016C RID: 364
		[SerializeField]
		private UIInputField conditionInputField;

		// Token: 0x0400016D RID: 365
		[SerializeField]
		private UIInputField stackTraceInputField;
	}
}
