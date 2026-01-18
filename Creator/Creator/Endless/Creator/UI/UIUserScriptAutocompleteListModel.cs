using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000175 RID: 373
	public class UIUserScriptAutocompleteListModel : UIBaseLocalFilterableListModel<UIUserScriptAutocompleteListModelItem>
	{
		// Token: 0x1700008B RID: 139
		// (get) Token: 0x0600058A RID: 1418 RVA: 0x0001D748 File Offset: 0x0001B948
		protected override Comparison<UIUserScriptAutocompleteListModelItem> DefaultSort
		{
			get
			{
				return (UIUserScriptAutocompleteListModelItem x, UIUserScriptAutocompleteListModelItem y) => string.Compare(x.Value, y.Value, StringComparison.Ordinal);
			}
		}
	}
}
