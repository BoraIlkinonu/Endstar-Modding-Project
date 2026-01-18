using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Endless.Creator.Notifications
{
	// Token: 0x02000317 RID: 791
	[Serializable]
	public class NotificationSaveList<T1, T2> where T1 : NotificationKey<T2> where T2 : NotificationStatus
	{
		// Token: 0x06000E4E RID: 3662 RVA: 0x00043B20 File Offset: 0x00041D20
		public NotificationSaveList()
		{
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x00043B3E File Offset: 0x00041D3E
		public NotificationSaveList(Dictionary<T1, T2> notifications)
		{
			this.Split(notifications);
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x00043B64 File Offset: 0x00041D64
		private void Split(Dictionary<T1, T2> notifications)
		{
			this.keys.Clear();
			this.values.Clear();
			foreach (KeyValuePair<T1, T2> keyValuePair in notifications)
			{
				if (keyValuePair.Value.Status != NotificationState.New)
				{
					this.keys.Add(keyValuePair.Key);
					this.values.Add(keyValuePair.Value);
				}
			}
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x00043BFC File Offset: 0x00041DFC
		public Dictionary<T1, T2> Combine()
		{
			Dictionary<T1, T2> dictionary = new Dictionary<T1, T2>();
			if (this.keys.Count != this.values.Count)
			{
				return dictionary;
			}
			for (int i = 0; i < this.keys.Count; i++)
			{
				dictionary.Add(this.keys[i], this.values[i]);
			}
			return dictionary;
		}

		// Token: 0x04000C21 RID: 3105
		[JsonProperty]
		private List<T1> keys = new List<T1>();

		// Token: 0x04000C22 RID: 3106
		[JsonProperty]
		private List<T2> values = new List<T2>();
	}
}
