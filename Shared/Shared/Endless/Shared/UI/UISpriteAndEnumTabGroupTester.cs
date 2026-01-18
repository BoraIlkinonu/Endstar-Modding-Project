using System;
using Endless.Core.UI;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000268 RID: 616
	public class UISpriteAndEnumTabGroupTester : UIGameObject
	{
		// Token: 0x06000F99 RID: 3993 RVA: 0x000431C4 File Offset: 0x000413C4
		private void Start()
		{
			if (this.sprites.Length != 0)
			{
				this.spriteAndEnumTabs.PopulateFromEnumWithSprites(this.defaultValue, this.sprites, false);
				return;
			}
			this.spriteAndEnumTabs.PopulateFromEnum(this.defaultValue, false);
		}

		// Token: 0x040009F0 RID: 2544
		[SerializeField]
		private UISpriteAndEnumTabGroup spriteAndEnumTabs;

		// Token: 0x040009F1 RID: 2545
		[SerializeField]
		private TextTypewriterEffects defaultValue;

		// Token: 0x040009F2 RID: 2546
		[SerializeField]
		private Sprite[] sprites = Array.Empty<Sprite>();
	}
}
