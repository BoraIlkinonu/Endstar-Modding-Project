using System.Collections.Generic;
using System.IO;
using Endless.Shared;
using Newtonsoft.Json;
using Runtime.Shared.Utilities;
using UnityEngine;

namespace Endless.Creator.Notifications;

public abstract class NotificationManager<T0, T1, T2> : MonoBehaviourSingleton<T0> where T0 : MonoBehaviour where T1 : NotificationKey<T2> where T2 : NotificationStatus
{
	private Dictionary<T1, T2> notifications = new Dictionary<T1, T2>();

	protected T2 AddOrUpdateNotification(T1 key, T2 status)
	{
		if (notifications.TryAdd(key, status))
		{
			return status;
		}
		if (key.CompareStatus(notifications[key], status))
		{
			notifications[key] = status;
			return status;
		}
		return notifications[key];
	}

	protected bool MarkNotificationSeen(T1 key)
	{
		if (notifications.TryGetValue(key, out var value) && value.Status != NotificationState.Seen)
		{
			value.Status = NotificationState.Seen;
			return true;
		}
		return false;
	}

	protected T2 GetNotification(T1 key)
	{
		return notifications.GetValueOrDefault(key);
	}

	protected bool HasNotification(T1 key)
	{
		return notifications.ContainsKey(key);
	}

	protected bool RemoveNotification(T1 key)
	{
		return notifications.Remove(key);
	}

	public void Save(string filename)
	{
		string safeSavePath = EndlessEnvironment.GetSafeSavePath();
		safeSavePath = Path.Combine(safeSavePath, filename);
		if (!safeSavePath.EndsWith(".json"))
		{
			safeSavePath += ".json";
		}
		Debug.Log("Saving Notifications To: " + safeSavePath);
		string contents = JsonConvert.SerializeObject(new NotificationSaveList<T1, T2>(notifications));
		File.WriteAllText(safeSavePath, contents);
		Debug.Log("Save Complete.");
	}

	public void Load(string filename)
	{
		ClearNotifications();
		string safeSavePath = EndlessEnvironment.GetSafeSavePath();
		safeSavePath = Path.Combine(safeSavePath, filename);
		if (!safeSavePath.EndsWith(".json"))
		{
			safeSavePath += ".json";
		}
		Debug.Log("Loading Notifications From: " + safeSavePath);
		if (File.Exists(safeSavePath))
		{
			NotificationSaveList<T1, T2> notificationSaveList = JsonConvert.DeserializeObject<NotificationSaveList<T1, T2>>(File.ReadAllText(safeSavePath));
			notifications = notificationSaveList.Combine();
			Debug.Log("Load Complete");
		}
	}

	protected void ClearNotifications()
	{
		notifications.Clear();
	}
}
