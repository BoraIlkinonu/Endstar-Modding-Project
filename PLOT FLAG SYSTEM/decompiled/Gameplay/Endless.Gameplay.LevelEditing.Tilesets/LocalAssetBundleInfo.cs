using System.Collections.Generic;
using Endless.Gameplay.VisualManagement;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class LocalAssetBundleInfo
{
	public int FileInstanceId;

	public int IconFileInstanceId;

	public string Name;

	public string AssetVersion;

	public Tileset GeneratedTileset;

	public List<MaterialManager> MaterialManagers;
}
