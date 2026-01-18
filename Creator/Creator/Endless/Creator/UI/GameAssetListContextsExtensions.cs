using System;
using System.Collections.Generic;

namespace Endless.Creator.UI
{
	// Token: 0x020000A7 RID: 167
	public static class GameAssetListContextsExtensions
	{
		// Token: 0x060002A6 RID: 678 RVA: 0x00011C8A File Offset: 0x0000FE8A
		public static bool CanEditRoles(this AssetContexts assetContext)
		{
			return GameAssetListContextsExtensions.canEditRolesHashSet.Contains(assetContext);
		}

		// Token: 0x040002DB RID: 731
		private static readonly HashSet<AssetContexts> canEditRolesHashSet = new HashSet<AssetContexts>
		{
			AssetContexts.MainMenu,
			AssetContexts.NewGame,
			AssetContexts.GameInspectorCreate,
			AssetContexts.GameOrLevelEditor
		};
	}
}
