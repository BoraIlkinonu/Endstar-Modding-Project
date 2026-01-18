using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Endless.Networking.GraphQL
{
	// Token: 0x02000026 RID: 38
	[Serializable]
	public class GraphQLResult
	{
		// Token: 0x1700003A RID: 58
		// (get) Token: 0x0600013A RID: 314 RVA: 0x000067AA File Offset: 0x000049AA
		// (set) Token: 0x0600013B RID: 315 RVA: 0x000067B2 File Offset: 0x000049B2
		public string RawResult { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x0600013C RID: 316 RVA: 0x000067BB File Offset: 0x000049BB
		public bool HasErrors
		{
			get
			{
				return this.errors != null;
			}
		}

		// Token: 0x0600013D RID: 317 RVA: 0x000067C8 File Offset: 0x000049C8
		public bool HasDataMember(string member)
		{
			return this.data.ContainsKey(member);
		}

		// Token: 0x0600013E RID: 318 RVA: 0x000067E8 File Offset: 0x000049E8
		public object GetDataMember(string member)
		{
			return this.data[member];
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00006808 File Offset: 0x00004A08
		public void DebugData()
		{
			foreach (KeyValuePair<string, object> keyValuePair in this.data)
			{
				Logger.Log(null, string.Format("{0} : {1}", keyValuePair.Key, keyValuePair.Value), true);
			}
		}

		// Token: 0x06000140 RID: 320 RVA: 0x0000687C File Offset: 0x00004A7C
		public string GetErrorMessage(int index = 0)
		{
			return this.errors[index]["message"].ToString();
		}

		// Token: 0x04000096 RID: 150
		[JsonProperty("data")]
		private Dictionary<string, object> data = new Dictionary<string, object>();

		// Token: 0x04000097 RID: 151
		[JsonProperty("errors")]
		private Dictionary<string, object>[] errors = null;
	}
}
