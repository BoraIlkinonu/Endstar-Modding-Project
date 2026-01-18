using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000163 RID: 355
	public class UIHorizontalLayoutGroup : UIHorizontalOrVerticalLayoutGroup
	{
		// Token: 0x06000876 RID: 2166 RVA: 0x000240E6 File Offset: 0x000222E6
		public override void CalculateLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("CalculateLayout", this);
			}
			base.CalcAlongAxis(UIBaseLayoutGroup.Axes.Horizontal, false);
			base.CalcAlongAxis(UIBaseLayoutGroup.Axes.Vertical, false);
		}

		// Token: 0x06000877 RID: 2167 RVA: 0x0002410B File Offset: 0x0002230B
		public override void ApplyLayout()
		{
			if (this.verboseLogging)
			{
				Debug.Log("ApplyLayout", this);
			}
			base.SetChildrenAlongAxis(UIBaseLayoutGroup.Axes.Horizontal, false);
			base.SetChildrenAlongAxis(UIBaseLayoutGroup.Axes.Vertical, false);
		}
	}
}
