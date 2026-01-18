using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200006B RID: 107
	public class UISettingsConfirmationModalController : UIGameObject
	{
		// Token: 0x060001F2 RID: 498 RVA: 0x0000B390 File Offset: 0x00009590
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.applyButton.onClick.AddListener(new UnityAction(this.Apply));
			this.revertButton.onClick.AddListener(new UnityAction(this.Revert));
		}

		// Token: 0x060001F3 RID: 499 RVA: 0x0000B3F0 File Offset: 0x000095F0
		private void Apply()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Apply", Array.Empty<object>());
			}
			this.view.SettingsVideoController.Apply();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			Action invokeOnAction = this.view.InvokeOnAction;
			if (invokeOnAction == null)
			{
				return;
			}
			invokeOnAction();
		}

		// Token: 0x060001F4 RID: 500 RVA: 0x0000B444 File Offset: 0x00009644
		private void Revert()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Revert", Array.Empty<object>());
			}
			this.view.SettingsVideoController.ReinitializeAll();
			MonoBehaviourSingleton<UIModalManager>.Instance.CloseAndClearStack();
			Action invokeOnAction = this.view.InvokeOnAction;
			if (invokeOnAction == null)
			{
				return;
			}
			invokeOnAction();
		}

		// Token: 0x04000165 RID: 357
		[SerializeField]
		private UISettingsConfirmationModalView view;

		// Token: 0x04000166 RID: 358
		[SerializeField]
		private UIButton applyButton;

		// Token: 0x04000167 RID: 359
		[SerializeField]
		private UIButton revertButton;

		// Token: 0x04000168 RID: 360
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
