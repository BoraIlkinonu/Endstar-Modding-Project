using System;
using Endless.Shared.Debugging;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000123 RID: 291
	[Serializable]
	public struct SpriteAndString
	{
		// Token: 0x17000136 RID: 310
		// (get) Token: 0x06000731 RID: 1841 RVA: 0x0001E689 File Offset: 0x0001C889
		// (set) Token: 0x06000732 RID: 1842 RVA: 0x0001E691 File Offset: 0x0001C891
		public Sprite Sprite { readonly get; private set; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x06000733 RID: 1843 RVA: 0x0001E69A File Offset: 0x0001C89A
		// (set) Token: 0x06000734 RID: 1844 RVA: 0x0001E6A2 File Offset: 0x0001C8A2
		public string String { readonly get; private set; }

		// Token: 0x06000735 RID: 1845 RVA: 0x0001E6AB File Offset: 0x0001C8AB
		public SpriteAndString(Sprite spriteValue, string stringValue)
		{
			this.Sprite = spriteValue;
			this.String = stringValue;
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x0001E6BB File Offset: 0x0001C8BB
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"{ Sprite: ",
				this.Sprite.DebugSafeName(true),
				", String: ",
				this.String,
				" }"
			});
		}
	}
}
