using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x0200029E RID: 670
	public class UIDockableToolPanelController : UIGameObject
	{
		// Token: 0x06000B2C RID: 2860 RVA: 0x00034638 File Offset: 0x00032838
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.dockButton.onClick.AddListener(new UnityAction(this.dockableToolPanelView.Interface.Dock));
			this.undockButton.onClick.AddListener(new UnityAction(this.dockableToolPanelView.Interface.Undock));
		}

		// Token: 0x0400096D RID: 2413
		[SerializeField]
		private InterfaceReference<IDockableToolPanelView> dockableToolPanelView;

		// Token: 0x0400096E RID: 2414
		[SerializeField]
		private UIButton dockButton;

		// Token: 0x0400096F RID: 2415
		[SerializeField]
		private UIButton undockButton;

		// Token: 0x04000970 RID: 2416
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
