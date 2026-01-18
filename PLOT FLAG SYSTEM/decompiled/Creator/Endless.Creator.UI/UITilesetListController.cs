using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI;

public class UITilesetListController : UIBaseLocalFilterableListController<Tileset>
{
	protected override bool IncludeInFilteredResults(Tileset item)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "IncludeInFilteredResults", item);
		}
		if (item == null)
		{
			DebugUtility.LogError("Tileset was null!", this);
			return false;
		}
		string text = item.DisplayName;
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
