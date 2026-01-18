using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000012 RID: 18
	[Serializable]
	public class FileAsset : Asset
	{
		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000044 RID: 68 RVA: 0x0000268B File Offset: 0x0000088B
		[JsonProperty("asset_need_update_parent_version")]
		public bool UpdateParentVersion
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000045 RID: 69 RVA: 0x0000268E File Offset: 0x0000088E
		public FileAsset()
		{
			this.AssetType = "screenshot";
		}

		// Token: 0x06000046 RID: 70 RVA: 0x000026AC File Offset: 0x000008AC
		public override object GetAnonymousObjectForUpload()
		{
			FileAsset fileAsset = JsonConvert.DeserializeObject<FileAsset>(JsonConvert.SerializeObject(this));
			fileAsset.AssetVersion = string.Empty;
			return fileAsset;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x000026C4 File Offset: 0x000008C4
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder(base.ToString());
			stringBuilder.Append(string.Format("\nFile Count: {0}", this.FileInstances.Count));
			for (int i = 0; i < this.FileInstances.Count; i++)
			{
				stringBuilder.Append(string.Format("\n File: Label - {0} Instance Id - {1}", this.FileInstances[i].Label, this.FileInstances[i].AssetFileInstanceId));
			}
			return stringBuilder.ToString();
		}

		// Token: 0x04000062 RID: 98
		[JsonProperty("file_instances")]
		public List<FileAssetInstance> FileInstances = new List<FileAssetInstance>();
	}
}
