using System;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x0200025F RID: 607
	[RequireComponent(typeof(Image))]
	public class UISpriteVariableHandler : UIGameObject
	{
		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06000F58 RID: 3928 RVA: 0x00042265 File Offset: 0x00040465
		private Image Image
		{
			get
			{
				if (!this.image)
				{
					base.TryGetComponent<Image>(out this.image);
				}
				return this.image;
			}
		}

		// Token: 0x06000F59 RID: 3929 RVA: 0x00042287 File Offset: 0x00040487
		public void ApplySpriteToImage()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "ApplySpriteToImage", Array.Empty<object>());
			}
			this.Image.sprite = this.sprite.Value;
		}

		// Token: 0x040009CE RID: 2510
		[SerializeField]
		private SpriteVariable sprite;

		// Token: 0x040009CF RID: 2511
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040009D0 RID: 2512
		private Image image;
	}
}
