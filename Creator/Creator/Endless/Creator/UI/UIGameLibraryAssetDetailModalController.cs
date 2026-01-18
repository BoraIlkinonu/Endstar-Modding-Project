using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020001B1 RID: 433
	public class UIGameLibraryAssetDetailModalController : UIGameObject
	{
		// Token: 0x06000669 RID: 1641 RVA: 0x00021500 File Offset: 0x0001F700
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.editButton.onClick.AddListener(new UnityAction(this.Edit));
			this.duplicateButton.onClick.AddListener(new UnityAction(this.Duplicate));
		}

		// Token: 0x0600066A RID: 1642 RVA: 0x0002155D File Offset: 0x0001F75D
		private void Edit()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Edit", Array.Empty<object>());
			}
			this.gameAssetDetail.SetWriteable(!this.gameAssetDetail.Writeable);
		}

		// Token: 0x0600066B RID: 1643 RVA: 0x00021590 File Offset: 0x0001F790
		private void Duplicate()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Duplicate", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x040005C0 RID: 1472
		[SerializeField]
		private UIButton editButton;

		// Token: 0x040005C1 RID: 1473
		[SerializeField]
		private UIButton duplicateButton;

		// Token: 0x040005C2 RID: 1474
		[SerializeField]
		private UIGameAssetDetailView gameAssetDetail;

		// Token: 0x040005C3 RID: 1475
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
