using System;
using System.Collections.Generic;
using Endless.Shared.Debugging;

namespace Endless.Shared.UI
{
	// Token: 0x020001A9 RID: 425
	public class UIInputFieldListModel : UIBaseListModel<string>
	{
		// Token: 0x17000213 RID: 531
		// (get) Token: 0x06000AF8 RID: 2808 RVA: 0x00030409 File Offset: 0x0002E609
		// (set) Token: 0x06000AF9 RID: 2809 RVA: 0x00030411 File Offset: 0x0002E611
		public string DefaultValueOnAdd { get; private set; } = string.Empty;

		// Token: 0x06000AFA RID: 2810 RVA: 0x0003041C File Offset: 0x0002E61C
		public List<T> GetCopyOfValuesAsObjects<T>()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCopyOfValuesAsObjects", Array.Empty<object>());
			}
			List<T> list = new List<T>();
			foreach (string text in base.ReadOnlyList)
			{
				T t = (T)((object)Convert.ChangeType(text, typeof(T)));
				list.Add(t);
			}
			return list;
		}
	}
}
