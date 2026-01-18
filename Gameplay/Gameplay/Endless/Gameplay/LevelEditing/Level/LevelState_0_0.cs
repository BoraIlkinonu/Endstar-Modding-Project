using System;
using System.Collections.Generic;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000558 RID: 1368
	[Serializable]
	public class LevelState_0_0 : LevelAsset
	{
		// Token: 0x17000649 RID: 1609
		// (get) Token: 0x060020F1 RID: 8433 RVA: 0x00017586 File Offset: 0x00015786
		[JsonProperty("asset_need_update_parent_version")]
		public bool UpdateParentVersion
		{
			get
			{
				return true;
			}
		}

		// Token: 0x04001A3B RID: 6715
		[JsonProperty]
		public List<PropEntry> propEntries = new List<PropEntry>();

		// Token: 0x04001A3C RID: 6716
		[JsonProperty]
		public List<TerrainEntry> terrainEntries = new List<TerrainEntry>();

		// Token: 0x04001A3D RID: 6717
		[JsonProperty]
		public List<WireBundle> wireBundles = new List<WireBundle>();

		// Token: 0x04001A3E RID: 6718
		[JsonProperty]
		public List<SerializableGuid> spawnPointIds = new List<SerializableGuid>();

		// Token: 0x04001A3F RID: 6719
		[JsonProperty]
		public List<SerializableGuid> selectedSpawnPointIds = new List<SerializableGuid>();

		// Token: 0x04001A40 RID: 6720
		[JsonProperty]
		public List<int> ScreenshotFileInstanceIds = new List<int>();

		// Token: 0x04001A41 RID: 6721
		[JsonProperty]
		public SerializableGuid defaultEnvironmentInstanceId = SerializableGuid.Empty;
	}
}
