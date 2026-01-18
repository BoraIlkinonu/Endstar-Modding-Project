using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x020001E0 RID: 480
	public class UIColorDefaultHdrView : UIColorDefaultView
	{
		// Token: 0x06000BC7 RID: 3015 RVA: 0x00032EE1 File Offset: 0x000310E1
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log("OnDestroy", this);
			}
			UIColorHdrUtility.SafeDestroyTexture(ref this.hdrColorTexture);
		}

		// Token: 0x06000BC8 RID: 3016 RVA: 0x00032F04 File Offset: 0x00031104
		protected override void ViewColorPreview(Color model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.Log(string.Format("{0} ( {1}: {2} )", "ViewColorPreview", "model", model), this);
			}
			this.hdrColorTexture = UIColorHdrUtility.ViewHdrColorPreview(model, this.colorPreviewRawImage, this.hdrColorTexture);
		}

		// Token: 0x0400079D RID: 1949
		[Header("UIColorDefaultHdrView")]
		[SerializeField]
		protected RawImage colorPreviewRawImage;

		// Token: 0x0400079E RID: 1950
		private Texture2D hdrColorTexture;
	}
}
