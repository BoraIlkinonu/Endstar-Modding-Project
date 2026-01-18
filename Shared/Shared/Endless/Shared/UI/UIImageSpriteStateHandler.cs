using System;
using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000147 RID: 327
	public class UIImageSpriteStateHandler : UIGameObject
	{
		// Token: 0x06000811 RID: 2065 RVA: 0x00021F48 File Offset: 0x00020148
		public void Set(bool state)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Set", new object[] { state });
			}
			foreach (UIImageSpriteStateHandler.UISpriteStateHandler uispriteStateHandler in this.spriteStates)
			{
				uispriteStateHandler.Set(state);
			}
		}

		// Token: 0x040004E5 RID: 1253
		[SerializeField]
		private UIImageSpriteStateHandler.UISpriteStateHandler[] spriteStates = new UIImageSpriteStateHandler.UISpriteStateHandler[0];

		// Token: 0x040004E6 RID: 1254
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x02000148 RID: 328
		[Serializable]
		private struct UISpriteStateHandler
		{
			// Token: 0x06000813 RID: 2067 RVA: 0x00021FB0 File Offset: 0x000201B0
			public void Set(bool state)
			{
				this.Image.sprite = (state ? this.Active : this.Inactive);
			}

			// Token: 0x040004E7 RID: 1255
			public Image Image;

			// Token: 0x040004E8 RID: 1256
			public Sprite Active;

			// Token: 0x040004E9 RID: 1257
			public Sprite Inactive;
		}
	}
}
