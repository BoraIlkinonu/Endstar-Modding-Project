using System;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Shared;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay
{
	// Token: 0x020000F2 RID: 242
	[Serializable]
	public class CellReference
	{
		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000557 RID: 1367 RVA: 0x0001B629 File Offset: 0x00019829
		[JsonIgnore]
		public bool HasValue
		{
			get
			{
				return this.Cell != null;
			}
		}

		// Token: 0x170000E4 RID: 228
		// (get) Token: 0x06000558 RID: 1368 RVA: 0x0001B636 File Offset: 0x00019836
		[JsonIgnore]
		public bool RotationHasValue
		{
			get
			{
				return this.Rotation != null;
			}
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x0001B643 File Offset: 0x00019843
		public Vector3Int GetCellPositionAsVector3Int()
		{
			if (this.Cell == null)
			{
				return Vector3Int.zero;
			}
			return Vector3Int.RoundToInt(this.Cell.Value);
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x0001B668 File Offset: 0x00019868
		public Vector3 GetCellPosition()
		{
			Vector3? cell = this.Cell;
			if (cell == null)
			{
				return Vector3.zero;
			}
			return cell.GetValueOrDefault();
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x0001B692 File Offset: 0x00019892
		internal void SetCell(Vector3? cell, float? rotation)
		{
			this.Cell = cell;
			this.Rotation = rotation;
		}

		// Token: 0x0600055C RID: 1372 RVA: 0x0001B6A4 File Offset: 0x000198A4
		internal Cell GetCell()
		{
			Vector3Int cellPositionAsVector3Int = this.GetCellPositionAsVector3Int();
			if (this.Cell == null)
			{
				return null;
			}
			return MonoBehaviourSingleton<StageManager>.Instance.ActiveStage.GetCellFromCoordinate(cellPositionAsVector3Int);
		}

		// Token: 0x0600055D RID: 1373 RVA: 0x0001B6D7 File Offset: 0x000198D7
		public float GetRotation()
		{
			return this.Rotation.GetValueOrDefault();
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x0001B6E4 File Offset: 0x000198E4
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3} }}", new object[]
			{
				"Cell",
				(this.Cell != null) ? this.Cell.Value : "null",
				"Rotation",
				(this.Rotation != null) ? this.Rotation.Value : "null"
			});
		}

		// Token: 0x0400041E RID: 1054
		[JsonProperty]
		internal Vector3? Cell;

		// Token: 0x0400041F RID: 1055
		[JsonProperty("Rot")]
		internal float? Rotation;
	}
}
