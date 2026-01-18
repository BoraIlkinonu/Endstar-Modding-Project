using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001AF RID: 431
	public class UIGameLibraryAssetAdditionModalController : UIGameObject
	{
		// Token: 0x06000664 RID: 1636 RVA: 0x000211C0 File Offset: 0x0001F3C0
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.gameAssetSourceDropdown.OnEnumValueChanged.AddListener(new UnityAction<Enum>(this.view.ViewSource));
		}

		// Token: 0x040005B8 RID: 1464
		[SerializeField]
		private UIGameLibraryAssetAdditionModalView view;

		// Token: 0x040005B9 RID: 1465
		[SerializeField]
		private UIDropdownEnum gameAssetSourceDropdown;

		// Token: 0x040005BA RID: 1466
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
