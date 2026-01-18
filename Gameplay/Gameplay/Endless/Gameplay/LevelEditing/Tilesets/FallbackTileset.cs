using System;
using Endless.Assets;
using Endless.Gameplay.LevelEditing.Level;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x02000512 RID: 1298
	public class FallbackTileset : Tileset
	{
		// Token: 0x1700060E RID: 1550
		// (get) Token: 0x06001F6F RID: 8047 RVA: 0x0008B55E File Offset: 0x0008975E
		public override string DisplayName { get; }

		// Token: 0x1700060F RID: 1551
		// (get) Token: 0x06001F70 RID: 8048 RVA: 0x0008B566 File Offset: 0x00089766
		public override Sprite DisplayIcon { get; }

		// Token: 0x06001F71 RID: 8049 RVA: 0x0008B570 File Offset: 0x00089770
		public FallbackTileset(Asset asset, Sprite displayIcon, GameObject fallback, int desiredIndex)
			: base(null, asset, displayIcon, desiredIndex)
		{
			this.DisplayIcon = displayIcon;
			TileCosmetic tileCosmetic = new TileCosmetic();
			tileCosmetic.AddVisual(fallback.transform);
			tileCosmetic.SetIndex(0);
			this.fallbackTile = new Tile(tileCosmetic);
		}

		// Token: 0x06001F72 RID: 8050 RVA: 0x0008B5B4 File Offset: 0x000897B4
		public override TileSpawnContext GetValidVisualForCellPosition(Stage stage, Vector3Int coordinates)
		{
			return new TileSpawnContext
			{
				Tileset = this,
				Tile = this.fallbackTile,
				AllowTopDecoration = false,
				TopFilled = false,
				BottomFilled = false,
				LeftFilled = false,
				RightFilled = false,
				FrontFilled = false,
				BackFilled = false
			};
		}

		// Token: 0x040018F4 RID: 6388
		private Tile fallbackTile;
	}
}
