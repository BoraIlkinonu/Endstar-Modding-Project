using System;
using System.Collections.Generic;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x0200055B RID: 1371
	[Serializable]
	public class MainMenuGameModel : Asset
	{
		// Token: 0x1700064D RID: 1613
		// (get) Token: 0x06002102 RID: 8450 RVA: 0x00094AE4 File Offset: 0x00092CE4
		public static string AssetReturnArgs
		{
			get
			{
				return "{ asset_id asset_version Name Description levels { asset_ref_id asset_ref_version } screenshots { thumbnail mainImage } revision_meta_data }";
			}
		}

		// Token: 0x1700064E RID: 1614
		// (get) Token: 0x06002103 RID: 8451 RVA: 0x00094AEB File Offset: 0x00092CEB
		// (set) Token: 0x06002104 RID: 8452 RVA: 0x00094AF3 File Offset: 0x00092CF3
		[JsonProperty("levels")]
		public List<AssetReference> Levels { get; private set; }

		// Token: 0x1700064F RID: 1615
		// (get) Token: 0x06002105 RID: 8453 RVA: 0x00094AFC File Offset: 0x00092CFC
		// (set) Token: 0x06002106 RID: 8454 RVA: 0x00094B04 File Offset: 0x00092D04
		[JsonProperty("screenshots")]
		public List<ScreenshotFileInstances> Screenshots { get; private set; } = new List<ScreenshotFileInstances>();
	}
}
