using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Endless.Creator.Notifications;

[Serializable]
public class NotificationSaveList<T1, T2> where T1 : NotificationKey<T2> where T2 : NotificationStatus
{
	[JsonProperty]
	private List<T1> keys = new List<T1>();

	[JsonProperty]
	private List<T2> values = new List<T2>();

	public NotificationSaveList()
	{
	}

	public NotificationSaveList(Dictionary<T1, T2> notifications)
	{
		Split(notifications);
	}

	private void Split(Dictionary<T1, T2> notifications)
	{
		keys.Clear();
		values.Clear();
		foreach (KeyValuePair<T1, T2> notification in notifications)
		{
			if (notification.Value.Status != NotificationState.New)
			{
				keys.Add(notification.Key);
				values.Add(notification.Value);
			}
		}
	}

	public Dictionary<T1, T2> Combine()
	{
		Dictionary<T1, T2> dictionary = new Dictionary<T1, T2>();
		if (keys.Count != values.Count)
		{
			return dictionary;
		}
		for (int i = 0; i < keys.Count; i++)
		{
			dictionary.Add(keys[i], values[i]);
		}
		return dictionary;
	}
}
