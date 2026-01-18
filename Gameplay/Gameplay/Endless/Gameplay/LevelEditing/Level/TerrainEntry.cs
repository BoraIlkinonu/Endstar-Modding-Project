using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000553 RID: 1363
	[Serializable]
	public class TerrainEntry
	{
		// Token: 0x060020DC RID: 8412 RVA: 0x000941C2 File Offset: 0x000923C2
		public TerrainEntry Copy()
		{
			return new TerrainEntry
			{
				Position = this.Position,
				TilesetId = this.TilesetId
			};
		}

		// Token: 0x04001A2A RID: 6698
		[JsonProperty("Pos")]
		public Vector3Int Position;

		// Token: 0x04001A2B RID: 6699
		public int TilesetId;
	}
}
