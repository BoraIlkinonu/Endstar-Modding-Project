using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000137 RID: 311
	[Serializable]
	public struct UIDropdownOption
	{
		// Token: 0x060007D5 RID: 2005 RVA: 0x00021276 File Offset: 0x0001F476
		public UIDropdownOption(string textValue, Sprite spriteValue)
		{
			this.TextValue = textValue;
			this.SpriteValue = spriteValue;
		}

		// Token: 0x060007D6 RID: 2006 RVA: 0x00021286 File Offset: 0x0001F486
		public UIDropdownOption(string textValue)
		{
			this.TextValue = textValue;
			this.SpriteValue = null;
		}

		// Token: 0x060007D7 RID: 2007 RVA: 0x00021296 File Offset: 0x0001F496
		public UIDropdownOption(Sprite spriteValue)
		{
			this.TextValue = null;
			this.SpriteValue = spriteValue;
		}

		// Token: 0x060007D8 RID: 2008 RVA: 0x000212A6 File Offset: 0x0001F4A6
		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}: {3}", new object[] { "TextValue", this.TextValue, "SpriteValue", this.SpriteValue });
		}

		// Token: 0x040004A7 RID: 1191
		public string TextValue;

		// Token: 0x040004A8 RID: 1192
		public Sprite SpriteValue;
	}
}
