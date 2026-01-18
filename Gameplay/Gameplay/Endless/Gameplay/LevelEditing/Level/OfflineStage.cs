using System;
using System.Collections.Generic;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200055D RID: 1373
	public class OfflineStage : MonoBehaviour
	{
		// Token: 0x17000650 RID: 1616
		// (get) Token: 0x0600210C RID: 8460 RVA: 0x00094EF1 File Offset: 0x000930F1
		// (set) Token: 0x0600210D RID: 8461 RVA: 0x00094EF9 File Offset: 0x000930F9
		public Transform TileRoot { get; private set; }

		// Token: 0x17000651 RID: 1617
		// (get) Token: 0x0600210E RID: 8462 RVA: 0x00094F02 File Offset: 0x00093102
		// (set) Token: 0x0600210F RID: 8463 RVA: 0x00094F0A File Offset: 0x0009310A
		public ChunkManager ChunkManager { get; private set; }

		// Token: 0x06002110 RID: 8464 RVA: 0x00094F13 File Offset: 0x00093113
		public bool TryGetCellFromCoordinate(Vector3Int coordinate, out Cell result)
		{
			return this.cellLookup.TryGetValue(coordinate, out result);
		}

		// Token: 0x06002111 RID: 8465 RVA: 0x00094F22 File Offset: 0x00093122
		public void AddCell(Vector3Int coordinate, Cell cell)
		{
			this.cellLookup.Add(coordinate, cell);
		}

		// Token: 0x06002112 RID: 8466 RVA: 0x00094F31 File Offset: 0x00093131
		public bool RemoveCell(Vector3Int coordinate)
		{
			return this.cellLookup.Remove(coordinate);
		}

		// Token: 0x06002113 RID: 8467 RVA: 0x00094F3F File Offset: 0x0009313F
		public IEnumerable<Cell> GetCells()
		{
			return this.cellLookup.Values;
		}

		// Token: 0x06002114 RID: 8468 RVA: 0x00094F4C File Offset: 0x0009314C
		public IEnumerable<Vector3Int> GetCellCoordinates()
		{
			return this.cellLookup.Keys;
		}

		// Token: 0x04001A50 RID: 6736
		private readonly Dictionary<Vector3Int, Cell> cellLookup = new Dictionary<Vector3Int, Cell>();
	}
}
