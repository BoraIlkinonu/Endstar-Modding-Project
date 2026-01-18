using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIUserScriptAutocompleteListModel : UIBaseLocalFilterableListModel<UIUserScriptAutocompleteListModelItem>
{
	protected override Comparison<UIUserScriptAutocompleteListModelItem> DefaultSort => (UIUserScriptAutocompleteListModelItem x, UIUserScriptAutocompleteListModelItem y) => string.Compare(x.Value, y.Value, StringComparison.Ordinal);
}
