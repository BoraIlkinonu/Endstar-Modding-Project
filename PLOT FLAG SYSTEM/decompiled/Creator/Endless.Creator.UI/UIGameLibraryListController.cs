using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIGameLibraryListController : UIBaseLocalFilterableListController<UIGameAsset>
{
	protected override bool IncludeInFilteredResults(UIGameAsset item)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "IncludeInFilteredResults", item);
		}
		string text = item.Name;
		if (!base.CaseSensitive)
		{
			text = text.ToLower();
		}
		string text2 = base.StringFilter;
		if (!base.CaseSensitive)
		{
			text2 = text2.ToLower();
		}
		return text.Contains(text2);
	}
}
