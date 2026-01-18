using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Endless.TerrainCosmetics;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x0200050A RID: 1290
	public class BaseFringeSet : FringeSet
	{
		// Token: 0x06001F5F RID: 8031 RVA: 0x0008ABA6 File Offset: 0x00088DA6
		public BaseFringeSet(BaseFringeCosmeticProfile cosmeticSet)
		{
			this.cosmeticSet = cosmeticSet;
		}

		// Token: 0x06001F60 RID: 8032 RVA: 0x0008ABB8 File Offset: 0x00088DB8
		public override void AddFringe(Transform parent, Vector3Int coordinate, TileSpawnContext context, Stage stage)
		{
			BaseFringeSet.<>c__DisplayClass3_0 CS$<>8__locals1;
			CS$<>8__locals1.stage = stage;
			CS$<>8__locals1.context = context;
			Cell cellFromCoordinate = CS$<>8__locals1.stage.GetCellFromCoordinate(coordinate + Vector3Int.up);
			if (cellFromCoordinate != null && cellFromCoordinate.BlocksBaseFringe())
			{
				return;
			}
			List<bool> list;
			if (!this.cosmeticSet.IsVerticalFringe)
			{
				list = new List<bool>
				{
					!CS$<>8__locals1.context.FrontFilled,
					!CS$<>8__locals1.context.RightFilled,
					!CS$<>8__locals1.context.BackFilled,
					!CS$<>8__locals1.context.LeftFilled
				};
			}
			else
			{
				list = this.GetValidFringeDirections(coordinate + new Vector3Int(0, 1, 0), CS$<>8__locals1.stage, CS$<>8__locals1.context);
			}
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				if (list[i])
				{
					num++;
				}
			}
			if (num == 1)
			{
				int num2 = list.IndexOf(true);
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.SingleFringe, num2, parent);
			}
			else if (num == 2)
			{
				if (list[0] && list[2])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.SingleFringe, 0, parent);
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.SingleFringe, 2, parent);
				}
				else if (list[1] && list[3])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.SingleFringe, 1, parent);
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.SingleFringe, 3, parent);
				}
				else if (list[0] && list[1])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.CornerFringe, 1, parent);
				}
				else if (list[1] && list[2])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.CornerFringe, 2, parent);
				}
				else if (list[2] && list[3])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.CornerFringe, 3, parent);
				}
				else if (list[3] && list[0])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.CornerFringe, 0, parent);
				}
			}
			else if (num == 3)
			{
				if (!list[0])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.PeninsulaFringe, 2, parent);
				}
				else if (!list[1])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.PeninsulaFringe, 3, parent);
				}
				else if (!list[2])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.PeninsulaFringe, 0, parent);
				}
				else if (!list[3])
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.PeninsulaFringe, 1, parent);
				}
			}
			else if (num == 4)
			{
				base.SpawnFringeWithRotationIndex(this.cosmeticSet.IslandFringe, 0, parent);
			}
			Vector3Int[] array = new Vector3Int[]
			{
				Vector3Int.forward + Vector3Int.right,
				Vector3Int.right + Vector3Int.back,
				Vector3Int.back + Vector3Int.left,
				Vector3Int.left + Vector3Int.forward
			};
			if (!this.cosmeticSet.CornerCapFringe)
			{
				return;
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (!list[j] && !list[(j + 1) % list.Count] && BaseFringeSet.<AddFringe>g__CanPlaceCornerCap|3_0(coordinate + array[j], ref CS$<>8__locals1))
				{
					base.SpawnFringeWithRotationIndex(this.cosmeticSet.CornerCapFringe, j, parent);
				}
			}
		}

		// Token: 0x06001F61 RID: 8033 RVA: 0x0008AF58 File Offset: 0x00089158
		private List<bool> GetValidFringeDirections(Vector3Int coordinate, Stage stage, TileSpawnContext contex)
		{
			TerrainCell cellFromCoordinateAs = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.forward);
			TerrainCell cellFromCoordinateAs2 = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.right);
			TerrainCell cellFromCoordinateAs3 = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.back);
			TerrainCell cellFromCoordinateAs4 = stage.GetCellFromCoordinateAs<TerrainCell>(coordinate + Vector3Int.left);
			return new List<bool>
			{
				contex.FrontFilled && cellFromCoordinateAs != null && !(stage.TilesetAtIndex(cellFromCoordinateAs.TilesetIndex) is SlopeTileset),
				contex.RightFilled && cellFromCoordinateAs2 != null && !(stage.TilesetAtIndex(cellFromCoordinateAs2.TilesetIndex) is SlopeTileset),
				contex.BackFilled && cellFromCoordinateAs3 != null && !(stage.TilesetAtIndex(cellFromCoordinateAs3.TilesetIndex) is SlopeTileset),
				contex.LeftFilled && cellFromCoordinateAs4 != null && !(stage.TilesetAtIndex(cellFromCoordinateAs4.TilesetIndex) is SlopeTileset)
			};
		}

		// Token: 0x06001F62 RID: 8034 RVA: 0x0008B060 File Offset: 0x00089260
		[CompilerGenerated]
		internal static bool <AddFringe>g__CanPlaceCornerCap|3_0(Vector3Int targetCellCoordinate, ref BaseFringeSet.<>c__DisplayClass3_0 A_1)
		{
			Cell cellFromCoordinate = A_1.stage.GetCellFromCoordinate(targetCellCoordinate);
			if (cellFromCoordinate == null || !(cellFromCoordinate is TerrainCell))
			{
				return true;
			}
			Tileset tileset = MonoBehaviourSingleton<StageManager>.Instance.ActiveTerrainPalette.GetTileset(cellFromCoordinate.TilesetIndex);
			int? num = ((tileset != null) ? new int?(tileset.Index) : null);
			int index = A_1.context.Tileset.Index;
			return !((num.GetValueOrDefault() == index) & (num != null));
		}

		// Token: 0x040018A8 RID: 6312
		private const int VALID_FRINGE_DIRECTION_COUNT = 4;

		// Token: 0x040018A9 RID: 6313
		private readonly BaseFringeCosmeticProfile cosmeticSet;
	}
}
