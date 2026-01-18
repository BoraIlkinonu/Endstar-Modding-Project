using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.Debugging;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000187 RID: 391
	public class UITilesetListController : UIBaseLocalFilterableListController<Tileset>
	{
		// Token: 0x060005BF RID: 1471 RVA: 0x0001DEA0 File Offset: 0x0001C0A0
		protected override bool IncludeInFilteredResults(Tileset item)
		{
			if (base.SuperVerboseLogging)
			{
				DebugUtility.LogMethod(this, "IncludeInFilteredResults", new object[] { item });
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
}
