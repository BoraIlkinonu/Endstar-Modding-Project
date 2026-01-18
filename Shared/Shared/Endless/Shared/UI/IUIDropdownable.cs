using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Endless.Shared.UI
{
	// Token: 0x02000132 RID: 306
	public interface IUIDropdownable
	{
		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000782 RID: 1922
		UnityEvent OnValueChanged { get; }

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000783 RID: 1923
		HashSet<int> ValueIndices { get; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x06000784 RID: 1924
		int OptionsCount { get; }

		// Token: 0x06000785 RID: 1925
		bool OptionShouldBeHidden(int index);

		// Token: 0x06000786 RID: 1926
		void ToggleValueIndex(int valueIndex, bool triggerValueChanged);

		// Token: 0x06000787 RID: 1927
		bool GetIsSelected(int index);
	}
}
