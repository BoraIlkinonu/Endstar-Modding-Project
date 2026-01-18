using System;
using UnityEngine;

namespace Endless.Shared.UI
{
	// Token: 0x02000150 RID: 336
	public interface IUILayoutable
	{
		// Token: 0x17000162 RID: 354
		// (get) Token: 0x06000830 RID: 2096
		RectTransform RectTransform { get; }

		// Token: 0x17000163 RID: 355
		// (get) Token: 0x06000831 RID: 2097
		int LayoutPriority { get; }

		// Token: 0x06000832 RID: 2098
		void RequestLayout();

		// Token: 0x06000833 RID: 2099
		void CalculateLayout();

		// Token: 0x06000834 RID: 2100
		void ApplyLayout();

		// Token: 0x06000835 RID: 2101
		void Layout();
	}
}
