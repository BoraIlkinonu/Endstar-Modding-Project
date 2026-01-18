using System;
using System.Collections.Generic;

namespace Endless.Shared.UI
{
	// Token: 0x02000201 RID: 513
	public static class IClearableExtensions
	{
		// Token: 0x06000D60 RID: 3424 RVA: 0x0003AE14 File Offset: 0x00039014
		public static void Clear(this List<IClearable> clearables)
		{
			foreach (IClearable clearable in clearables)
			{
				clearable.Clear();
			}
			clearables.Clear();
		}
	}
}
