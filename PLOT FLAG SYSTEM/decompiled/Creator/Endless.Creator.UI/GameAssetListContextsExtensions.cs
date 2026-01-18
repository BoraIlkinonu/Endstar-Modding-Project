using System.Collections.Generic;

namespace Endless.Creator.UI;

public static class GameAssetListContextsExtensions
{
	private static readonly HashSet<AssetContexts> canEditRolesHashSet = new HashSet<AssetContexts>
	{
		AssetContexts.MainMenu,
		AssetContexts.NewGame,
		AssetContexts.GameInspectorCreate,
		AssetContexts.GameOrLevelEditor
	};

	public static bool CanEditRoles(this AssetContexts assetContext)
	{
		return canEditRolesHashSet.Contains(assetContext);
	}
}
