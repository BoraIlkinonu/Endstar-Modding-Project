using System;
using Endless.Gameplay;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x0200013B RID: 315
	public class UILevelDestinationSelectionListView : UIBaseListView<LevelDestination>
	{
		// Token: 0x1700007D RID: 125
		// (get) Token: 0x060004EC RID: 1260 RVA: 0x0001BBCE File Offset: 0x00019DCE
		// (set) Token: 0x060004ED RID: 1261 RVA: 0x0001BBD6 File Offset: 0x00019DD6
		public UILevelDestinationSelectionListView.SelectionTypes SelectionType { get; private set; }

		// Token: 0x0200013C RID: 316
		public enum SelectionTypes
		{
			// Token: 0x0400048B RID: 1163
			ApplyToProperty,
			// Token: 0x0400048C RID: 1164
			LocalListToggleSelected
		}
	}
}
