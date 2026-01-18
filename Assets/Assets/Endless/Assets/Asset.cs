using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Endless.Assets
{
	// Token: 0x02000005 RID: 5
	[Serializable]
	public class Asset : AssetCore
	{
		// Token: 0x0600000F RID: 15 RVA: 0x000021B8 File Offset: 0x000003B8
		public virtual object GetAnonymousObjectForUpload()
		{
			Asset asset = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this), base.GetType()) as Asset;
			asset.AssetVersion = "";
			return asset;
		}

		// Token: 0x06000010 RID: 16 RVA: 0x000021DC File Offset: 0x000003DC
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", new object[]
			{
				"Description",
				this.Description,
				"InternalVersion",
				this.InternalVersion,
				"RevisionMetaData",
				this.RevisionMetaData,
				"AssetCore",
				base.ToString()
			});
		}

		// Token: 0x04000009 RID: 9
		[JsonProperty]
		public string Description;

		// Token: 0x0400000A RID: 10
		[JsonProperty("internal_version")]
		[HideInInspector]
		public string InternalVersion = "0.0.0";

		// Token: 0x0400000B RID: 11
		[JsonProperty("revision_meta_data")]
		public RevisionMetaData RevisionMetaData = new RevisionMetaData();
	}
}
