using System;
using Endless.Creator.DynamicPropCreation;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001CD RID: 461
	public class UIPropCreationPromptDataModalView : UIBaseModalView
	{
		// Token: 0x060006E0 RID: 1760 RVA: 0x00022D34 File Offset: 0x00020F34
		public override void OnDisplay(params object[] modalData)
		{
			base.OnDisplay(modalData);
			PropCreationPromptData propCreationPromptData = (PropCreationPromptData)modalData[0];
			this.text.text = propCreationPromptData.Message;
		}

		// Token: 0x060006E1 RID: 1761 RVA: 0x0001FDD0 File Offset: 0x0001DFD0
		public override void OnBack()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnBack", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseWithoutClearingStack();
		}

		// Token: 0x0400062C RID: 1580
		[Header("UIPropCreationPromptDataModalView")]
		[SerializeField]
		private TextMeshProUGUI text;
	}
}
