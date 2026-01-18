using System;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200058B RID: 1419
	[Serializable]
	public class TerrainChange
	{
		// Token: 0x04001B4D RID: 6989
		public Vector3Int[] Coordinates = new Vector3Int[0];

		// Token: 0x04001B4E RID: 6990
		public int TilesetIndex;

		// Token: 0x04001B4F RID: 6991
		public bool Erased;
	}
}
