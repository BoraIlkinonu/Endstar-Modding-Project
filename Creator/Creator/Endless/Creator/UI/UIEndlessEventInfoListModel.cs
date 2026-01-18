using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000F1 RID: 241
	public class UIEndlessEventInfoListModel : UIBaseLocalFilterableListModel<EndlessEventInfo>
	{
		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060003F8 RID: 1016 RVA: 0x0001913F File Offset: 0x0001733F
		// (set) Token: 0x060003F9 RID: 1017 RVA: 0x00019147 File Offset: 0x00017347
		public UIEndlessEventInfoListModel.Types Type { get; private set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060003FA RID: 1018 RVA: 0x00019150 File Offset: 0x00017350
		protected override Comparison<EndlessEventInfo> DefaultSort
		{
			get
			{
				return (EndlessEventInfo x, EndlessEventInfo y) => string.Compare(x.MemberName, y.MemberName, StringComparison.Ordinal);
			}
		}

		// Token: 0x020000F2 RID: 242
		public enum Types
		{
			// Token: 0x04000410 RID: 1040
			Emitter,
			// Token: 0x04000411 RID: 1041
			Receiver
		}
	}
}
