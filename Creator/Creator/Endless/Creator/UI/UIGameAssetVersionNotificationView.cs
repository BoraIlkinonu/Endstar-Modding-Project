using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x02000261 RID: 609
	public class UIGameAssetVersionNotificationView : UIGameObject
	{
		// Token: 0x060009E6 RID: 2534 RVA: 0x0002D970 File Offset: 0x0002BB70
		public void View(GameEditorAssetVersionManager.UpdateStatus updateStatus)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { updateStatus });
			}
			this.upToDate.gameObject.SetActive(updateStatus == GameEditorAssetVersionManager.UpdateStatus.UpToDate);
			this.seenUpdate.gameObject.SetActive(updateStatus == GameEditorAssetVersionManager.UpdateStatus.SeenUpdate);
			this.newUpdate.gameObject.SetActive(updateStatus == GameEditorAssetVersionManager.UpdateStatus.NewUpdate);
		}

		// Token: 0x0400081D RID: 2077
		[SerializeField]
		private GameObject upToDate;

		// Token: 0x0400081E RID: 2078
		[SerializeField]
		private GameObject seenUpdate;

		// Token: 0x0400081F RID: 2079
		[SerializeField]
		private GameObject newUpdate;

		// Token: 0x04000820 RID: 2080
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
