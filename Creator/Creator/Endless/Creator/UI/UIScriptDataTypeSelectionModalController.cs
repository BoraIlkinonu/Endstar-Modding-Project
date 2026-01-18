using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001D2 RID: 466
	public class UIScriptDataTypeSelectionModalController : UIGameObject
	{
		// Token: 0x060006FD RID: 1789 RVA: 0x000235E8 File Offset: 0x000217E8
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.scriptDataTypeRadio.OnValueChanged.AddListener(new UnityAction<ScriptDataTypes>(this.view.ViewScriptDataTypeInfo));
			this.continueButton.onClick.AddListener(new UnityAction(this.Continue));
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x0002364A File Offset: 0x0002184A
		private void Continue()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Continue", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIScriptDataWizard>.Instance.SetScriptDataType(this.scriptDataTypeRadio.Value);
		}

		// Token: 0x0400064A RID: 1610
		[SerializeField]
		private UIScriptDataTypeSelectionModalView view;

		// Token: 0x0400064B RID: 1611
		[SerializeField]
		private UIScriptDataTypeRadio scriptDataTypeRadio;

		// Token: 0x0400064C RID: 1612
		[SerializeField]
		private UIButton continueButton;

		// Token: 0x0400064D RID: 1613
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
