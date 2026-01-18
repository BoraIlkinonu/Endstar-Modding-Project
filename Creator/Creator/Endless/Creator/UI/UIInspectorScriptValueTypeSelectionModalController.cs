using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001D0 RID: 464
	public class UIInspectorScriptValueTypeSelectionModalController : UIGameObject
	{
		// Token: 0x060006F5 RID: 1781 RVA: 0x000233BC File Offset: 0x000215BC
		private void Awake()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Awake", Array.Empty<object>());
			}
			this.inspectorScriptValueTypeRadio.OnValueChanged.AddListener(new UnityAction<Type>(this.view.ViewInspectorScriptValueTypeInfo));
			this.inspectorScriptValueTypeRadio.OnValueChanged.AddListener(new UnityAction<Type>(this.view.HandleIsCollectionToggleVisibility));
			this.continueButton.onClick.AddListener(new UnityAction(this.Continue));
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00023440 File Offset: 0x00021640
		private void Continue()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Continue", Array.Empty<object>());
			}
			MonoBehaviourSingleton<UIModalManager>.Instance.Display(this.inspectorScriptValueInputModalSource, UIModalManagerStackActions.MaintainStack, new object[]
			{
				this.inspectorScriptValueTypeRadio.Value,
				this.isCollectionToggle.IsOn
			});
		}

		// Token: 0x0400063E RID: 1598
		[SerializeField]
		private UIInspectorScriptValueTypeSelectionModalView view;

		// Token: 0x0400063F RID: 1599
		[SerializeField]
		private UIInspectorScriptValueTypeRadio inspectorScriptValueTypeRadio;

		// Token: 0x04000640 RID: 1600
		[SerializeField]
		private UIToggle isCollectionToggle;

		// Token: 0x04000641 RID: 1601
		[SerializeField]
		private UIButton continueButton;

		// Token: 0x04000642 RID: 1602
		[SerializeField]
		private UIInspectorScriptValueInputModalView inspectorScriptValueInputModalSource;

		// Token: 0x04000643 RID: 1603
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
