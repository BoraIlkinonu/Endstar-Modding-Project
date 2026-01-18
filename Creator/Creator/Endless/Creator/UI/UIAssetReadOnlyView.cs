using System;
using Endless.Assets;
using Endless.Shared.Debugging;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI
{
	// Token: 0x020000A3 RID: 163
	public class UIAssetReadOnlyView<T> : UIAssetView<T> where T : Asset
	{
		// Token: 0x06000295 RID: 661 RVA: 0x000119EC File Offset: 0x0000FBEC
		public override void View(T model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "View", "model", model), this);
			}
			this.nameText.text = model.Name;
			this.descriptionText.text = model.Description;
		}

		// Token: 0x06000296 RID: 662 RVA: 0x00011A4D File Offset: 0x0000FC4D
		public override void Clear()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("Clear", this);
			}
			this.nameText.text = string.Empty;
			this.descriptionText.text = string.Empty;
		}

		// Token: 0x040002CC RID: 716
		[SerializeField]
		private TextMeshProUGUI descriptionText;

		// Token: 0x040002CD RID: 717
		[SerializeField]
		private TextMeshProUGUI nameText;
	}
}
