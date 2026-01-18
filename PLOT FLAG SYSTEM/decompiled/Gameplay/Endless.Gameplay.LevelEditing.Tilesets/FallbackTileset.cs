using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets;

public class FallbackTileset : Tileset
{
	private Tile fallbackTile;

	public override string DisplayName { get; }

	public override Sprite DisplayIcon { get; }

	public FallbackTileset(Asset asset, Sprite displayIcon, GameObject fallback, int desiredIndex)
		: base(null, asset, displayIcon, desiredIndex)
	{
		DisplayIcon = displayIcon;
		TileCosmetic tileCosmetic = new TileCosmetic();
		tileCosmetic.AddVisual(fallback.transform);
		tileCosmetic.SetIndex(0);
		fallbackTile = new Tile(tileCosmetic);
	}

	public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
	{
		return new TileSpawnContext
		{
			Tileset = this,
			Tile = fallbackTile,
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
