using System;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

[Serializable]
public class TilesetCosmeticProfileRequest
{
	public AssetBundle AssetBundle;

	public TerrainTilesetCosmeticAsset Asset;

	public int FileInstanceId;
}
