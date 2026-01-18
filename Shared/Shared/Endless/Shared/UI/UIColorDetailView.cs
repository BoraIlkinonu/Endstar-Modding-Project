using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001E4 RID: 484
	public class UIColorDetailView : UIBaseColorDetailView
	{
		// Token: 0x17000234 RID: 564
		// (get) Token: 0x06000C04 RID: 3076 RVA: 0x00033E2B File Offset: 0x0003202B
		protected override int ColorMax
		{
			get
			{
				return 255;
			}
		}

		// Token: 0x06000C05 RID: 3077 RVA: 0x0003431F File Offset: 0x0003251F
		protected override void ViewColorPreview(Color model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewColorPreview", "model", model), this);
			}
			this.colorPreviewImage.color = model;
		}

		// Token: 0x040007C1 RID: 1985
		[Header("UIColorDetailView")]
		[SerializeField]
		private Image colorPreviewImage;
	}
}
