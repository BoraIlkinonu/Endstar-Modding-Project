using System;

namespace Endless.Shared.UI
{
	// Token: 0x020001AF RID: 431
	public class UILogsListModel : UIBaseLocalFilterableListModel<UILog>
	{
		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06000B10 RID: 2832 RVA: 0x0003077E File Offset: 0x0002E97E
		protected override Comparison<UILog> DefaultSort
		{
			get
			{
				return (UILog x, UILog y) => x.Index.CompareTo(y.Index);
			}
		}
	}
}
