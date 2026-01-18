using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020001BD RID: 445
	[RequireComponent(typeof(UILevelStateSelectionModalView))]
	public class UILevelStateSelectionModalController : UIGameObject
	{
		// Token: 0x0600069D RID: 1693 RVA: 0x00021FCB File Offset: 0x000201CB
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UILevelStateSelectionModalView>(out this.view);
		}

		// Token: 0x040005EC RID: 1516
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040005ED RID: 1517
		private UILevelStateSelectionModalView view;
	}
}
