using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000122 RID: 290
	public readonly struct SpriteAndEnum
	{
		// Token: 0x0600072F RID: 1839 RVA: 0x0001E63F File Offset: 0x0001C83F
		public SpriteAndEnum(Sprite spriteValue, Enum enumValue)
		{
			this.Sprite = spriteValue;
			this.Enum = enumValue;
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x0001E64F File Offset: 0x0001C84F
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3} }}", new object[]
			{
				"Sprite",
				this.Sprite.DebugSafeName(true),
				"Enum",
				this.Enum
			});
		}

		// Token: 0x04000431 RID: 1073
		public readonly Sprite Sprite;

		// Token: 0x04000432 RID: 1074
		public readonly Enum Enum;
	}
}
