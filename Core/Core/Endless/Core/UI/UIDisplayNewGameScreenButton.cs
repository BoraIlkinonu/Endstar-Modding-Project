using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200007F RID: 127
	[RequireComponent(typeof(UIButton))]
	public class UIDisplayNewGameScreenButton : UIGameObject
	{
		// Token: 0x06000288 RID: 648 RVA: 0x0000E0A4 File Offset: 0x0000C2A4
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			UIButton uibutton;
			base.TryGetComponent<UIButton>(out uibutton);
			uibutton.onClick.AddListener(new UnityAction(this.Display));
		}

		// Token: 0x06000289 RID: 649 RVA: 0x0000E0E9 File Offset: 0x0000C2E9
		private void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			UINewGameScreenView.Display(UIScreenManager.DisplayStackActions.Push);
		}

		// Token: 0x040001DA RID: 474
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
