using System;
using Newtonsoft.Json;

namespace Endless.Assets
{
	// Token: 0x02000015 RID: 21
	[Serializable]
	public class ChangeData
	{
		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600004D RID: 77 RVA: 0x000027BD File Offset: 0x000009BD
		// (set) Token: 0x0600004E RID: 78 RVA: 0x000027C5 File Offset: 0x000009C5
		[JsonIgnore]
		public ChangeType ChangeType
		{
			get
			{
				return (ChangeType)this.changeType;
			}
			set
			{
				this.changeType = (int)value;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600004F RID: 79 RVA: 0x000027CE File Offset: 0x000009CE
		// (set) Token: 0x06000050 RID: 80 RVA: 0x000027D6 File Offset: 0x000009D6
		[JsonProperty("user_id")]
		public int UserId { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000027DF File Offset: 0x000009DF
		// (set) Token: 0x06000052 RID: 82 RVA: 0x000027E7 File Offset: 0x000009E7
		[JsonIgnore]
		public string Metadata
		{
			get
			{
				return this.metadata;
			}
			set
			{
				this.metadata = value;
			}
		}

		// Token: 0x06000053 RID: 83 RVA: 0x000027F0 File Offset: 0x000009F0
		public override bool Equals(object obj)
		{
			ChangeData changeData = (ChangeData)obj;
			return this.ChangeType.Equals(changeData.ChangeType) && this.UserId.Equals(changeData.UserId) && this.Metadata == changeData.Metadata;
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00002854 File Offset: 0x00000A54
		public override int GetHashCode()
		{
			int num = this.UserId.GetHashCode() ^ this.ChangeType.GetHashCode();
			if (this.Metadata != null)
			{
				num ^= this.Metadata.GetHashCode();
			}
			return num;
		}

		// Token: 0x06000055 RID: 85 RVA: 0x0000289C File Offset: 0x00000A9C
		public ChangeData Copy()
		{
			return new ChangeData
			{
				changeType = this.changeType,
				metadata = this.metadata,
				UserId = this.UserId
			};
		}

		// Token: 0x04000066 RID: 102
		[JsonProperty("change_type")]
		private int changeType;

		// Token: 0x04000067 RID: 103
		[JsonProperty("metadata")]
		private string metadata;
	}
}
