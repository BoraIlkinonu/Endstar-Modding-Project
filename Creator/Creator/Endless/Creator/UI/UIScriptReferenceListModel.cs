using System;
using Endless.Props.Assets;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200017B RID: 379
	public class UIScriptReferenceListModel : UIBaseLocalFilterableListModel<ScriptReference>
	{
		// Token: 0x1700008C RID: 140
		// (get) Token: 0x06000597 RID: 1431 RVA: 0x0001D852 File Offset: 0x0001BA52
		protected override Comparison<ScriptReference> DefaultSort
		{
			get
			{
				return (ScriptReference x, ScriptReference y) => string.Compare(x.NameInCode, y.NameInCode, StringComparison.Ordinal);
			}
		}
	}
}
