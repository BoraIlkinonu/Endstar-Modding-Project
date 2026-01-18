using Endless.Gameplay.LevelEditing;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoListController : UIBaseLocalFilterableListController<PropLibrary.RuntimePropInfo>
{
	protected override bool IncludeInFilteredResults(PropLibrary.RuntimePropInfo item)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "IncludeInFilteredResults", item);
		}
		if (item == null)
		{
			DebugUtility.LogError("PropLibrary.RuntimePropInfo was null!", this);
			return false;
		}
		string text = item.PropData.Name;
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
