using System;
using System.Collections.Generic;
using Endless.Gameplay.VisualManagement;

namespace Endless.Gameplay.LevelEditing.Tilesets
{
	// Token: 0x0200051A RID: 1306
	public class LocalAssetBundleInfo
	{
		// Token: 0x040018FC RID: 6396
		public int FileInstanceId;

		// Token: 0x040018FD RID: 6397
		public int IconFileInstanceId;

		// Token: 0x040018FE RID: 6398
		public string Name;

		// Token: 0x040018FF RID: 6399
		public string AssetVersion;

		// Token: 0x04001900 RID: 6400
		public Tileset GeneratedTileset;

		// Token: 0x04001901 RID: 6401
		public List<MaterialManager> MaterialManagers;
	}
}
