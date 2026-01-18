using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class PillarTileset : Tileset
{
	public PillarTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
		: base(cosmeticProfile, asset, displayIcon, index)
	{
	}

	public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
	{
		int num = -1;
		Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinates + Vector3Int.down);
		bool bottomFilled = false;
		bool topFilled = false;
		if (cellFromCoordinate == null || !ConsidersTileset(stage.GetTilesetIdFromCell(cellFromCoordinate)))
		{
			num = 0;
		}
		else
		{
			bottomFilled = true;
			Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinates + Vector3Int.up);
			if (cellFromCoordinate2 == null || !ConsidersTileset(stage.GetTilesetIdFromCell(cellFromCoordinate2)))
			{
				num = 2;
			}
			else
			{
				topFilled = true;
				num = 1;
			}
		}
		return new TileSpawnContext
		{
			Tileset = this,
			Tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(num)),
			AllowTopDecoration = false,
			TopFilled = topFilled,
			BottomFilled = bottomFilled,
			LeftFilled = false,
			RightFilled = false,
			FrontFilled = false,
			BackFilled = false
		};
	}
}
