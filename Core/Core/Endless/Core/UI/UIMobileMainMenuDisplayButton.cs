using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000065 RID: 101
	public class UIMobileMainMenuDisplayButton : UIGameObject
	{
		// Token: 0x060001DC RID: 476 RVA: 0x0000B052 File Offset: 0x00009252
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayMainMenuButton.onClick.AddListener(new UnityAction(this.Display));
		}

		// Token: 0x060001DD RID: 477 RVA: 0x0000B088 File Offset: 0x00009288
		private void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			UIMainMenuScreenView.Display(UIScreenManager.DisplayStackActions.ClearAndPush);
		}

		// Token: 0x04000153 RID: 339
		[SerializeField]
		private UIButton displayMainMenuButton;

		// Token: 0x04000154 RID: 340
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
