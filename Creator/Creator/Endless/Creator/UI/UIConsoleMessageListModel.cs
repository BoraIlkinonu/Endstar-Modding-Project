using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x020000EA RID: 234
	public class UIConsoleMessageListModel : UIBaseLocalFilterableListModel<UIConsoleMessageModel>
	{
		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060003E9 RID: 1001 RVA: 0x00018F4E File Offset: 0x0001714E
		protected override Comparison<UIConsoleMessageModel> DefaultSort { get; } = (UIConsoleMessageModel left, UIConsoleMessageModel right) => left.Message.Timestamp.CompareTo(right.Message.Timestamp);
	}
}
