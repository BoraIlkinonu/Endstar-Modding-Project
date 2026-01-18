using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000269 RID: 617
	public class UISpriteAndStringTab : UIBaseTab<SpriteAndString>
	{
		// Token: 0x06000F9B RID: 3995 RVA: 0x00043218 File Offset: 0x00041418
		protected override void ViewOption(SpriteAndString option)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ViewOption", new object[] { option });
			}
			this.image.gameObject.SetActive(option.Sprite);
			this.text.gameObject.SetActive(!option.String.IsNullOrEmptyOrWhiteSpace());
			this.image.sprite = option.Sprite;
			this.text.Value = option.String;
		}

		// Token: 0x040009F3 RID: 2547
		[Header("UISpriteAndStringTab")]
		[SerializeField]
		private Image image;

		// Token: 0x040009F4 RID: 2548
		[SerializeField]
		private UIText text;
	}
}
