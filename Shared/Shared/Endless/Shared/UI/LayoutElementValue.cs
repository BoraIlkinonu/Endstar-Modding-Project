using System;
using UnityEngine.UI;

namespace Endless.Shared.UI
{
	// Token: 0x02000151 RID: 337
	[Serializable]
	public class LayoutElementValue
	{
		// Token: 0x06000836 RID: 2102 RVA: 0x0002243C File Offset: 0x0002063C
		public void ApplyTo(LayoutElement layoutElement)
		{
			layoutElement.minWidth = this.MinWidth;
			layoutElement.minHeight = this.MinHeight;
			layoutElement.preferredWidth = this.PreferredWidth;
			layoutElement.preferredHeight = this.PreferredHeight;
			layoutElement.flexibleWidth = this.FlexibleWidth;
			layoutElement.flexibleHeight = this.FlexibleHeight;
			layoutElement.layoutPriority = this.LayoutPriority;
			layoutElement.ignoreLayout = this.IgnoreLayout;
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x000224AC File Offset: 0x000206AC
		public void CopyFrom(LayoutElement layoutElement)
		{
			this.MinWidth = layoutElement.minWidth;
			this.MinHeight = layoutElement.minHeight;
			this.PreferredWidth = layoutElement.preferredWidth;
			this.PreferredHeight = layoutElement.preferredHeight;
			this.FlexibleWidth = layoutElement.flexibleWidth;
			this.FlexibleHeight = layoutElement.flexibleHeight;
			this.LayoutPriority = layoutElement.layoutPriority;
			this.IgnoreLayout = layoutElement.ignoreLayout;
		}

		// Token: 0x040004EA RID: 1258
		public float MinWidth = -1f;

		// Token: 0x040004EB RID: 1259
		public float MinHeight = -1f;

		// Token: 0x040004EC RID: 1260
		public float PreferredWidth = -1f;

		// Token: 0x040004ED RID: 1261
		public float PreferredHeight = -1f;

		// Token: 0x040004EE RID: 1262
		public float FlexibleWidth = -1f;

		// Token: 0x040004EF RID: 1263
		public float FlexibleHeight = -1f;

		// Token: 0x040004F0 RID: 1264
		public int LayoutPriority = 1;

		// Token: 0x040004F1 RID: 1265
		public bool IgnoreLayout;
	}
}
