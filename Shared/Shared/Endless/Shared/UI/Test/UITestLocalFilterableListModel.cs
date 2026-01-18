using System;

namespace Endless.Shared.UI.Test
{
	// Token: 0x0200029E RID: 670
	public class UITestLocalFilterableListModel : UIBaseLocalFilterableListModel<int>
	{
		// Token: 0x1700032F RID: 815
		// (get) Token: 0x0600109E RID: 4254 RVA: 0x00046DD7 File Offset: 0x00044FD7
		protected override Comparison<int> DefaultSort
		{
			get
			{
				return (int x, int y) => x.CompareTo(y);
			}
		}
	}
}
