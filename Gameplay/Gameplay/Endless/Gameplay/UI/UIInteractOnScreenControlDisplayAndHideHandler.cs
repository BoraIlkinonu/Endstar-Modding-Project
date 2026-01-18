using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000404 RID: 1028
	public class UIInteractOnScreenControlDisplayAndHideHandler : UIGameObject
	{
		// Token: 0x060019B2 RID: 6578 RVA: 0x0007628C File Offset: 0x0007448C
		private void OnEnable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnEnable", Array.Empty<object>());
			}
			UIInteractionPromptAnchor.OnInitializeAction = (Action)Delegate.Combine(UIInteractionPromptAnchor.OnInitializeAction, new Action(this.displayAndHideHandler.Display));
			UIInteractionPromptAnchor.OnCloseAction = (Action)Delegate.Combine(UIInteractionPromptAnchor.OnCloseAction, new Action(this.displayAndHideHandler.Hide));
		}

		// Token: 0x060019B3 RID: 6579 RVA: 0x000762FB File Offset: 0x000744FB
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.displayAndHideHandler.SetToHideEnd(true);
		}

		// Token: 0x060019B4 RID: 6580 RVA: 0x00076324 File Offset: 0x00074524
		private void OnDisable()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDisable", Array.Empty<object>());
			}
			UIInteractionPromptAnchor.OnInitializeAction = (Action)Delegate.Remove(UIInteractionPromptAnchor.OnInitializeAction, new Action(this.displayAndHideHandler.Display));
			UIInteractionPromptAnchor.OnCloseAction = (Action)Delegate.Remove(UIInteractionPromptAnchor.OnCloseAction, new Action(this.displayAndHideHandler.Hide));
		}

		// Token: 0x04001464 RID: 5220
		[SerializeField]
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x04001465 RID: 5221
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
