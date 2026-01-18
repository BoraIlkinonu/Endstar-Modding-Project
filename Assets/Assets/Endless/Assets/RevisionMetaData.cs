using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000016 RID: 22
	[Serializable]
	public class RevisionMetaData
	{
		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000057 RID: 87 RVA: 0x000028CF File Offset: 0x00000ACF
		// (set) Token: 0x06000058 RID: 88 RVA: 0x000028D7 File Offset: 0x00000AD7
		[JsonProperty("revision_timestamp")]
		public long RevisionTimestamp { get; set; }

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000059 RID: 89 RVA: 0x000028E0 File Offset: 0x00000AE0
		[JsonProperty("changes")]
		public HashSet<ChangeData> Changes { get; } = new HashSet<ChangeData>();

		// Token: 0x0600005A RID: 90 RVA: 0x000028E8 File Offset: 0x00000AE8
		public RevisionMetaData()
		{
			this.RevisionTimestamp = DateTime.UtcNow.Ticks;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x0000291C File Offset: 0x00000B1C
		public RevisionMetaData(HashSet<ChangeData> initialChanges)
		{
			this.RevisionTimestamp = DateTime.UtcNow.Ticks;
			this.Changes = initialChanges;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00002954 File Offset: 0x00000B54
		public RevisionMetaData Copy()
		{
			RevisionMetaData revisionMetaData = new RevisionMetaData
			{
				RevisionTimestamp = this.RevisionTimestamp
			};
			foreach (ChangeData changeData in this.Changes)
			{
				revisionMetaData.Changes.Add(changeData.Copy());
			}
			return revisionMetaData;
		}
	}
}
