using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003B8 RID: 952
	public class UICharacterCosmeticsDefinitionListController : UIBaseLocalFilterableListController<CharacterCosmeticsDefinition>
	{
		// Token: 0x06001856 RID: 6230 RVA: 0x000710A0 File Offset: 0x0006F2A0
		protected override bool IncludeInFilteredResults(CharacterCosmeticsDefinition item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
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
}
