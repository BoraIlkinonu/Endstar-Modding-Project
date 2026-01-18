using System;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000265 RID: 613
	public interface IUITabGroup
	{
		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000F8E RID: 3982
		UnityEvent<int> OnValueChangedWithIndex { get; }

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06000F8F RID: 3983
		int ValueIndex { get; }

		// Token: 0x170002F0 RID: 752
		// (get) Token: 0x06000F90 RID: 3984
		int OptionsLength { get; }
	}
}
