using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000516 RID: 1302
	public class LoadingPlaceholderTileset : Tileset
	{
		// Token: 0x06001F7B RID: 8059 RVA: 0x0008B90C File Offset: 0x00089B0C
		public LoadingPlaceholderTileset(GameObject terrainVisual, Asset asset, Sprite displayIcon, int index)
			: base(null, asset, displayIcon, index)
		{
			TileCosmetic tileCosmetic = new TileCosmetic();
			tileCosmetic.AddVisual(terrainVisual.transform);
			tileCosmetic.SetIndex(0);
			this.loadingTile = new Tile(tileCosmetic);
		}

		// Token: 0x06001F7C RID: 8060 RVA: 0x0008B94C File Offset: 0x00089B4C
		public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
		{
			return new TileSpawnContext
			{
				Tileset = this,
				Tile = this.loadingTile,
				AllowTopDecoration = false,
				TopFilled = false,
				BottomFilled = false,
				LeftFilled = false,
				RightFilled = false,
				FrontFilled = false,
				BackFilled = false
			};
		}

		// Token: 0x040018F6 RID: 6390
		private Tile loadingTile;
	}
}
