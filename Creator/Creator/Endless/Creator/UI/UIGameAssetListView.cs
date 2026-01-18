using System;
using Endless.Shared.UI;

namespace Endless.Creator.UI
{
	// Token: 0x02000110 RID: 272
	public class UIGameAssetListView : UIBaseRoleInteractableListView<UIGameAsset>
	{
		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000458 RID: 1112 RVA: 0x00019FAA File Offset: 0x000181AA
		// (set) Token: 0x06000459 RID: 1113 RVA: 0x00019FB2 File Offset: 0x000181B2
		public UIGameAssetListView.SelectActions SelectAction { get; private set; } = UIGameAssetListView.SelectActions.ViewDetails;

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x0600045A RID: 1114 RVA: 0x00019FBB File Offset: 0x000181BB
		// (set) Token: 0x0600045B RID: 1115 RVA: 0x00019FC3 File Offset: 0x000181C3
		public bool ViewInLibraryMarker { get; private set; }

		// Token: 0x02000111 RID: 273
		public enum SelectActions
		{
			// Token: 0x04000436 RID: 1078
			ListSelect,
			// Token: 0x04000437 RID: 1079
			StaticSelect,
			// Token: 0x04000438 RID: 1080
			ViewDetails
		}
	}
}
