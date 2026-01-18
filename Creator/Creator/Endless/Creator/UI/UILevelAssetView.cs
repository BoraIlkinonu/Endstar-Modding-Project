using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000E2 RID: 226
	public class UILevelAssetView : UIGameObject
	{
		// Token: 0x060003C9 RID: 969 RVA: 0x00018428 File Offset: 0x00016628
		public void View(LevelAsset model)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			bool flag = model.Screenshots.Count > 0;
			this.defaultVisual.SetActive(!flag);
			this.mainScreenshot.gameObject.SetActive(flag);
			if (flag)
			{
				this.mainScreenshot.Display(model.Screenshots[0]);
			}
			this.nameText.text = model.Name;
		}

		// Token: 0x060003CA RID: 970 RVA: 0x000184AC File Offset: 0x000166AC
		public void Clear()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Clear", Array.Empty<object>());
			}
			this.defaultVisual.SetActive(true);
			this.mainScreenshot.gameObject.SetActive(false);
			this.mainScreenshot.Clear();
		}

		// Token: 0x040003E5 RID: 997
		[SerializeField]
		private GameObject defaultVisual;

		// Token: 0x040003E6 RID: 998
		[SerializeField]
		private UIScreenshotView mainScreenshot;

		// Token: 0x040003E7 RID: 999
		[SerializeField]
		private TextMeshProUGUI nameText;

		// Token: 0x040003E8 RID: 1000
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
