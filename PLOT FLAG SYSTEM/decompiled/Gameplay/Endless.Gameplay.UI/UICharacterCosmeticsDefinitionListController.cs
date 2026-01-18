using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI;

public class UICharacterCosmeticsDefinitionListController : UIBaseLocalFilterableListController<CharacterCosmeticsDefinition>
{
	protected override bool IncludeInFilteredResults(CharacterCosmeticsDefinition item)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "IncludeInFilteredResults", item);
		}
		if (item == null)
		{
			DebugUtility.LogError("CharacterCosmeticsDefinition was null!", this);
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
