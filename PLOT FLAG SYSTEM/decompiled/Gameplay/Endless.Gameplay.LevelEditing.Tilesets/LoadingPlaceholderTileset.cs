using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class LoadingPlaceholderTileset : Tileset
{
	private Tile loadingTile;

	public LoadingPlaceholderTileset(GameObject terrainVisual, Asset asset, Sprite displayIcon, int index)
		: base(null, asset, displayIcon, index)
	{
		TileCosmetic tileCosmetic = new TileCosmetic();
		tileCosmetic.AddVisual(terrainVisual.transform);
		tileCosmetic.SetIndex(0);
		loadingTile = new Tile(tileCosmetic);
	}

	public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
	{
		return new TileSpawnContext
		{
			Tileset = this,
			Tile = loadingTile,
			AllowTopDecoration = false,
			TopFilled = false,
			BottomFilled = false,
			LeftFilled = false,
			RightFilled = false,
			FrontFilled = false,
			BackFilled = false
		};
	}
}
