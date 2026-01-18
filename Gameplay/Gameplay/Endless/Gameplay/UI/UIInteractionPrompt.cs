using System;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Gameplay.UI
{
	// Token: 0x02000388 RID: 904
	[Serializable]
	public struct UIInteractionPrompt
	{
		// Token: 0x0400127C RID: 4732
		public UIDeviceTypeSpriteDictionary InteractionResultSprite;

		// Token: 0x0400127D RID: 4733
		public Color InteractionResultColor;

		// Token: 0x0400127E RID: 4734
		public string InteractionResultText;

		// Token: 0x0400127F RID: 4735
		public Sprite supplementalInteractionResultSprite;
	}
}
