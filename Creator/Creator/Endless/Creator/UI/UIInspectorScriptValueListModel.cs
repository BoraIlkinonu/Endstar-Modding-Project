using System;
using Endless.Props.Scripting;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000120 RID: 288
	public class UIInspectorScriptValueListModel : UIBaseLocalFilterableListModel<InspectorScriptValue>
	{
		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000488 RID: 1160 RVA: 0x0001A6AA File Offset: 0x000188AA
		protected override Comparison<InspectorScriptValue> DefaultSort
		{
			get
			{
				return (InspectorScriptValue x, InspectorScriptValue y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal);
			}
		}
	}
}
