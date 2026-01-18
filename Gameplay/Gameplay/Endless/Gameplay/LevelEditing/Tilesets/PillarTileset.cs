using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000517 RID: 1303
	public class PillarTileset : Tileset
	{
		// Token: 0x06001F7D RID: 8061 RVA: 0x0008B0DC File Offset: 0x000892DC
		public PillarTileset(TilesetCosmeticProfile cosmeticProfile, Asset asset, Sprite displayIcon, int index)
			: base(cosmeticProfile, asset, displayIcon, index)
		{
		}

		// Token: 0x06001F7E RID: 8062 RVA: 0x0008B9A4 File Offset: 0x00089BA4
		public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
		{
			Cell cellFromCoordinate = stage.GetCellFromCoordinate(coordinates + Vector3Int.down);
			bool flag = false;
			bool flag2 = false;
			int num;
			if (cellFromCoordinate == null || !base.ConsidersTileset(stage.GetTilesetIdFromCell(cellFromCoordinate)))
			{
				num = 0;
			}
			else
			{
				flag = true;
				Cell cellFromCoordinate2 = stage.GetCellFromCoordinate(coordinates + Vector3Int.up);
				if (cellFromCoordinate2 == null || !base.ConsidersTileset(stage.GetTilesetIdFromCell(cellFromCoordinate2)))
				{
					num = 2;
				}
				else
				{
					flag2 = true;
					num = 1;
				}
			}
			return new TileSpawnContext
			{
				Tileset = this,
				Tile = new Tile(base.CosmeticProfile.GetTileCosmeticAtIndex(num)),
				AllowTopDecoration = false,
				TopFilled = flag2,
				BottomFilled = flag,
				LeftFilled = false,
				RightFilled = false,
				FrontFilled = false,
				BackFilled = false
			};
		}
	}
}
