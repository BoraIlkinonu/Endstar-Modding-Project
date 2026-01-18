using System;
using Endless.Gameplay.LevelEditing.Tilesets;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000188 RID: 392
	public class UITilesetListModel : UIBaseLocalFilterableListModel<Tileset>
	{
		// Token: 0x17000093 RID: 147
		// (get) Token: 0x060005C1 RID: 1473 RVA: 0x0001DF15 File Offset: 0x0001C115
		// (set) Token: 0x060005C2 RID: 1474 RVA: 0x0001DF1D File Offset: 0x0001C11D
		public bool IsPaintTool { get; private set; }

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x060005C3 RID: 1475 RVA: 0x0001DF26 File Offset: 0x0001C126
		protected override Comparison<Tileset> DefaultSort
		{
			get
			{
				return (Tileset x, Tileset y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal);
			}
		}
	}
}
