using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000183 RID: 387
	public class UISpawnPointListView : UIBaseListView<UISpawnPoint>
	{
		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060005B3 RID: 1459 RVA: 0x0001DC54 File Offset: 0x0001BE54
		// (set) Token: 0x060005B4 RID: 1460 RVA: 0x0001DC5C File Offset: 0x0001BE5C
		public bool CanSelect { get; private set; } = true;
	}
}
