using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000165 RID: 357
	public class UIVerticalLayoutGroup : UIHorizontalOrVerticalLayoutGroup
	{
		// Token: 0x06000887 RID: 2183 RVA: 0x00024A50 File Offset: 0x00022C50
		public override void CalculateLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateLayout", this);
			}
			base.CalcAlongAxis(UIBaseLayoutGroup.Axes.Horizontal, true);
			base.CalcAlongAxis(UIBaseLayoutGroup.Axes.Vertical, true);
		}

		// Token: 0x06000888 RID: 2184 RVA: 0x00024A75 File Offset: 0x00022C75
		public override void ApplyLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("ApplyLayout", this);
			}
			base.SetChildrenAlongAxis(UIBaseLayoutGroup.Axes.Horizontal, true);
			base.SetChildrenAlongAxis(UIBaseLayoutGroup.Axes.Vertical, true);
		}
	}
}
