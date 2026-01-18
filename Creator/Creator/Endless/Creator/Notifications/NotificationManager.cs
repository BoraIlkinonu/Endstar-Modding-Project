using System;
using System.Collections.Generic;
using System.IO;
using Endless.Shared;
using Newtonsoft.Json;
using Runtime.Shared.Utilities;
using UnityEngine;

namespace Endless.Creator.Notifications
{
	// Token: 0x02000318 RID: 792
	public abstract class NotificationManager<T0, T1, T2> : MonoBehaviourSingleton<T0> where T0 : MonoBehaviour where T1 : NotificationKey<T2> where T2 : NotificationStatus
	{
		// Token: 0x06000E52 RID: 3666 RVA: 0x00043C60 File Offset: 0x00041E60
		protected T2 AddOrUpdateNotification(T1 key, T2 status)
		{
			if (this.notifications.TryAdd(key, status))
			{
				return status;
			}
			if (key.CompareStatus(this.notifications[key], status))
			{
				this.notifications[key] = status;
				return status;
			}
			return this.notifications[key];
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x00043CB4 File Offset: 0x00041EB4
		protected bool MarkNotificationSeen(T1 key)
		{
			T2 t;
			if (this.notifications.TryGetValue(key, out t) && t.Status != NotificationState.Seen)
			{
				t.Status = NotificationState.Seen;
				return true;
			}
			return false;
		}

		// Token: 0x06000E54 RID: 3668 RVA: 0x00043CED File Offset: 0x00041EED
		protected T2 GetNotification(T1 key)
		{
			return this.notifications.GetValueOrDefault(key);
		}

		// Token: 0x06000E55 RID: 3669 RVA: 0x00043CFB File Offset: 0x00041EFB
		protected bool HasNotification(T1 key)
		{
			return this.notifications.ContainsKey(key);
		}

		// Token: 0x06000E56 RID: 3670 RVA: 0x00043D09 File Offset: 0x00041F09
		protected bool RemoveNotification(T1 key)
		{
			return this.notifications.Remove(key);
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x00043D18 File Offset: 0x00041F18
		public void Save(string filename)
		{
			string text = EndlessEnvironment.GetSafeSavePath();
			text = Path.Combine(text, filename);
			if (!text.EndsWith(".json"))
			{
				text += ".json";
			}
			Debug.Log("Saving Notifications To: " + text);
			string text2 = JsonConvert.SerializeObject(new NotificationSaveList<T1, T2>(this.notifications));
			File.WriteAllText(text, text2);
			Debug.Log("Save Complete.");
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x00043D80 File Offset: 0x00041F80
		public void Load(string filename)
		{
			this.ClearNotifications();
			string text = EndlessEnvironment.GetSafeSavePath();
			text = Path.Combine(text, filename);
			if (!text.EndsWith(".json"))
			{
				text += ".json";
			}
			Debug.Log("Loading Notifications From: " + text);
			if (!File.Exists(text))
			{
				return;
			}
			NotificationSaveList<T1, T2> notificationSaveList = JsonConvert.DeserializeObject<NotificationSaveList<T1, T2>>(File.ReadAllText(text));
			this.notifications = notificationSaveList.Combine();
			Debug.Log("Load Complete");
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x00043DF5 File Offset: 0x00041FF5
		protected void ClearNotifications()
		{
			this.notifications.Clear();
		}

		// Token: 0x04000C23 RID: 3107
		private Dictionary<T1, T2> notifications = new Dictionary<T1, T2>();
	}
}
