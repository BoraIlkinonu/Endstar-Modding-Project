using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x0200010C RID: 268
	public class UIBadge : UIGameObject
	{
		// Token: 0x1700010C RID: 268
		// (get) Token: 0x0600066D RID: 1645 RVA: 0x0001B989 File Offset: 0x00019B89
		public string BadgeText
		{
			get
			{
				return this.badgeText.Value;
			}
		}

		// Token: 0x0600066E RID: 1646 RVA: 0x0001B996 File Offset: 0x00019B96
		public void Display(string text)
		{
			this.badgeText.Value = text;
		}

		// Token: 0x040003B6 RID: 950
		[SerializeField]
		private UIText badgeText;
	}
}
