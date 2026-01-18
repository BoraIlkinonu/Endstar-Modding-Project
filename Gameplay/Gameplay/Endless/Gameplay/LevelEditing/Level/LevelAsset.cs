using System;
using System.Collections.Generic;
using Endless.Assets;
using Newtonsoft.Json;

namespace Endless.Gameplay.LevelEditing.Level
{
	// Token: 0x02000544 RID: 1348
	[Serializable]
	public class LevelAsset : Asset
	{
		// Token: 0x17000637 RID: 1591
		// (get) Token: 0x06002081 RID: 8321 RVA: 0x00092594 File Offset: 0x00090794
		// (set) Token: 0x06002082 RID: 8322 RVA: 0x0009259C File Offset: 0x0009079C
		[JsonProperty("screenshots")]
		public List<ScreenshotFileInstances> Screenshots { get; set; } = new List<ScreenshotFileInstances>();

		// Token: 0x17000638 RID: 1592
		// (get) Token: 0x06002083 RID: 8323 RVA: 0x000925A5 File Offset: 0x000907A5
		// (set) Token: 0x06002084 RID: 8324 RVA: 0x000925AD File Offset: 0x000907AD
		public bool Archived { get; set; }

		// Token: 0x17000639 RID: 1593
		// (get) Token: 0x06002085 RID: 8325 RVA: 0x000925B6 File Offset: 0x000907B6
		public static string QueryString
		{
			get
			{
				return "game { levels { asset_id asset_version Name Description RevisionMetaData Archived screenshots { thumbnail mainImage } } }";
			}
		}

		// Token: 0x06002086 RID: 8326 RVA: 0x000925BD File Offset: 0x000907BD
		public LevelAsset()
		{
			this.AssetType = "level";
			this.InternalVersion = LevelState.INTERNAL_VERSION.ToString();
		}

		// Token: 0x06002087 RID: 8327 RVA: 0x000925EC File Offset: 0x000907EC
		public LevelAsset(LevelState levelState)
		{
			this.AssetType = "level";
			this.AssetID = levelState.AssetID;
			this.AssetVersion = levelState.AssetVersion;
			this.Name = levelState.Name;
			this.Description = levelState.Description;
			this.InternalVersion = LevelState.INTERNAL_VERSION.ToString();
		}

		// Token: 0x06002088 RID: 8328 RVA: 0x00092655 File Offset: 0x00090855
		public override object GetAnonymousObjectForUpload()
		{
			LevelState levelState = JsonConvert.DeserializeObject<LevelState>(JsonConvert.SerializeObject(this));
			levelState.AssetVersion = string.Empty;
			return levelState;
		}
	}
}
