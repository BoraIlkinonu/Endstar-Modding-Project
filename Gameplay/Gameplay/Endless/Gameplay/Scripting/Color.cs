using System;
using UnityEngine;

namespace Endless.Gameplay.Scripting
{
	// Token: 0x02000493 RID: 1171
	public class Color
	{
		// Token: 0x1700058D RID: 1421
		// (get) Token: 0x06001C8C RID: 7308 RVA: 0x0007DD25 File Offset: 0x0007BF25
		internal static Color Instance
		{
			get
			{
				if (Color.instance == null)
				{
					Color.instance = new Color();
				}
				return Color.instance;
			}
		}

		// Token: 0x06001C8D RID: 7309 RVA: 0x0007DD3D File Offset: 0x0007BF3D
		public Color Create(float red, float green, float blue, float alpha)
		{
			return new Color(red, green, blue, alpha);
		}

		// Token: 0x1700058E RID: 1422
		// (get) Token: 0x06001C8E RID: 7310 RVA: 0x0007DD49 File Offset: 0x0007BF49
		public Color Red
		{
			get
			{
				return Color.red;
			}
		}

		// Token: 0x1700058F RID: 1423
		// (get) Token: 0x06001C8F RID: 7311 RVA: 0x0007DD50 File Offset: 0x0007BF50
		public Color Black
		{
			get
			{
				return Color.black;
			}
		}

		// Token: 0x17000590 RID: 1424
		// (get) Token: 0x06001C90 RID: 7312 RVA: 0x0007DD57 File Offset: 0x0007BF57
		public Color Blue
		{
			get
			{
				return Color.blue;
			}
		}

		// Token: 0x17000591 RID: 1425
		// (get) Token: 0x06001C91 RID: 7313 RVA: 0x0007DD5E File Offset: 0x0007BF5E
		public Color Green
		{
			get
			{
				return Color.green;
			}
		}

		// Token: 0x17000592 RID: 1426
		// (get) Token: 0x06001C92 RID: 7314 RVA: 0x0007DD65 File Offset: 0x0007BF65
		public Color Clear
		{
			get
			{
				return Color.clear;
			}
		}

		// Token: 0x17000593 RID: 1427
		// (get) Token: 0x06001C93 RID: 7315 RVA: 0x0007DD6C File Offset: 0x0007BF6C
		public Color Cyan
		{
			get
			{
				return Color.cyan;
			}
		}

		// Token: 0x17000594 RID: 1428
		// (get) Token: 0x06001C94 RID: 7316 RVA: 0x0007DD73 File Offset: 0x0007BF73
		public Color Gray
		{
			get
			{
				return Color.gray;
			}
		}

		// Token: 0x17000595 RID: 1429
		// (get) Token: 0x06001C95 RID: 7317 RVA: 0x0007DD7A File Offset: 0x0007BF7A
		public Color Magenta
		{
			get
			{
				return Color.magenta;
			}
		}

		// Token: 0x17000596 RID: 1430
		// (get) Token: 0x06001C96 RID: 7318 RVA: 0x0007DD81 File Offset: 0x0007BF81
		public Color White
		{
			get
			{
				return Color.white;
			}
		}

		// Token: 0x17000597 RID: 1431
		// (get) Token: 0x06001C97 RID: 7319 RVA: 0x0007DD88 File Offset: 0x0007BF88
		public Color Yellow
		{
			get
			{
				return Color.yellow;
			}
		}

		// Token: 0x040016A8 RID: 5800
		private static Color instance;
	}
}
