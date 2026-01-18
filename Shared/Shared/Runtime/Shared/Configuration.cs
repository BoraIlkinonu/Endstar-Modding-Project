using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Runtime.Shared
{
	// Token: 0x02000007 RID: 7
	[Serializable]
	public class Configuration
	{
		// Token: 0x0600004C RID: 76 RVA: 0x0000445A File Offset: 0x0000265A
		public void AddOrOverwrite(string key, object value)
		{
			if (this.Values.ContainsKey(key))
			{
				this.Values[key] = value;
				return;
			}
			this.Values.Add(key, value);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00004488 File Offset: 0x00002688
		public bool TryGetValue<T>(string key, out T result)
		{
			object obj;
			if (this.Values.TryGetValue(key, out obj))
			{
				result = (T)((object)obj);
				return true;
			}
			result = default(T);
			return false;
		}

		// Token: 0x04000009 RID: 9
		[JsonProperty]
		public Dictionary<string, object> Values = new Dictionary<string, object>();
	}
}
