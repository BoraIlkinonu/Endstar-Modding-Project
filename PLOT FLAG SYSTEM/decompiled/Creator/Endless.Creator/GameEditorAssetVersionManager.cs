using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Endless.Assets;
using Endless.Creator.Notifications;
using Endless.Gameplay;
using Endless.Gameplay.LevelEditing.Level;
using Endless.Props.Assets;
using Endless.Shared.DataTypes;
using Endless.TerrainCosmetics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Endless.Creator;

public class GameEditorAssetVersionManager : NotificationManager<GameEditorAssetVersionManager, AssetUpdatedNotificationKey, AssetUpdatedNotificationStatus>
{
	public enum UpdateStatus
	{
		UpToDate,
		SeenUpdate,
		NewUpdate
	}

	private const string BASE_FILE_NAME = "GameEditorAssetVersionNotifications";

	[FormerlySerializedAs("OnNumberOfUpdatesChanged")]
	[HideInInspector]
	public UnityEvent<int> OnNumberOfNotificationsChanged = new UnityEvent<int>();

	private int notificationCount;

	private Dictionary<AssetUpdatedNotificationKey, List<Action<UpdateStatus>>> callbackMap = new Dictionary<AssetUpdatedNotificationKey, List<Action<UpdateStatus>>>();

	private string activeGameId;

	public async void LoadAssetUpdateStates(Game game, CancellationToken cancelToken)
	{
		activeGameId = game.AssetID;
		notificationCount = 0;
		Load("GameEditorAssetVersionNotifications-" + activeGameId);
		BulkAssetCacheResult<Prop> bulkPropResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(game.GameLibrary.PropReferences.Select((AssetReference reference) => ((SerializableGuid, string))(reference.AssetID, "")).ToArray());
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		SerializableGuid gameAssetID = game.AssetID;
		foreach (Prop asset in bulkPropResult.Assets)
		{
			if (game.GameLibrary.PropReferences.First((AssetReference reference) => reference.AssetID == asset.AssetID).AssetVersion != asset.AssetVersion)
			{
				ProcessAssetReferenceVersion(gameAssetID, asset);
			}
		}
		List<AssetReference> activeTerrainReferences = game.GetActiveTerrainReferences();
		BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(activeTerrainReferences.Select((AssetReference reference) => ((SerializableGuid, string))(reference.AssetID, "")).ToArray());
		if (cancelToken.IsCancellationRequested)
		{
			return;
		}
		foreach (TerrainTilesetCosmeticAsset asset2 in bulkAssetCacheResult.Assets)
		{
			if (activeTerrainReferences.First((AssetReference reference) => reference.AssetID == asset2.AssetID).AssetVersion != asset2.AssetVersion)
			{
				ProcessAssetReferenceVersion(gameAssetID, asset2);
			}
		}
		if (bulkPropResult.HasErrors)
		{
			Debug.LogException(new NotImplementedException("We have no error handling here!", bulkPropResult.GetErrorMessage()));
		}
		BroadcastNotificationCountUpdated();
	}

	private void ProcessAssetReferenceVersion(SerializableGuid gameId, Asset prop)
	{
		AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, prop.AssetID);
		AssetUpdatedNotificationStatus assetUpdatedNotificationStatus = AddOrUpdateNotification(assetUpdatedNotificationKey, new AssetUpdatedNotificationStatus(SemanticVersion.Parse(prop.AssetVersion)));
		if (assetUpdatedNotificationStatus.Status == NotificationState.New)
		{
			notificationCount++;
			BroadcastAssetStatus(assetUpdatedNotificationKey, UpdateStatus.NewUpdate);
		}
		else if (assetUpdatedNotificationStatus.Status == NotificationState.Seen)
		{
			BroadcastAssetStatus(assetUpdatedNotificationKey, UpdateStatus.SeenUpdate);
		}
	}

	private void BroadcastAssetStatus(AssetUpdatedNotificationKey notificationKey, UpdateStatus statusToBroadcast)
	{
		if (!callbackMap.TryGetValue(notificationKey, out var value))
		{
			return;
		}
		foreach (Action<UpdateStatus> item in value)
		{
			try
			{
				item(statusToBroadcast);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}

	public int GetNumberOfNotifications(Game game)
	{
		return notificationCount;
	}

	public void SubscribeToAssetVersionUpdated(SerializableGuid gameId, SerializableGuid assetId, Action<UpdateStatus> callback)
	{
		AssetUpdatedNotificationKey key = new AssetUpdatedNotificationKey(gameId, assetId);
		if (!callbackMap.ContainsKey(key))
		{
			callbackMap.Add(key, new List<Action<UpdateStatus>>());
		}
		callbackMap[key].Add(callback);
		if (HasNotification(key))
		{
			AssetUpdatedNotificationStatus notification = GetNotification(key);
			if (notification.Status == NotificationState.New)
			{
				callback(UpdateStatus.NewUpdate);
			}
			else if (notification.Status == NotificationState.Seen)
			{
				callback(UpdateStatus.SeenUpdate);
			}
		}
		else
		{
			callback(UpdateStatus.UpToDate);
		}
	}

	public void UnsubscribeToAssetVersionUpdated(SerializableGuid gameId, SerializableGuid assetId, Action<UpdateStatus> callback)
	{
		AssetUpdatedNotificationKey key = new AssetUpdatedNotificationKey(gameId, assetId);
		if (callbackMap.ContainsKey(key))
		{
			callbackMap[key].Remove(callback);
			if (callbackMap[key].Count == 0)
			{
				callbackMap.Remove(key);
			}
		}
	}

	public void SetAssetUpdateSeen(SerializableGuid gameId, SerializableGuid assetId)
	{
		SetAssetUpdateSeen(gameId, assetId, broadcastCountChanged: true);
	}

	private void SetAssetUpdateSeen(SerializableGuid gameId, SerializableGuid assetId, bool broadcastCountChanged)
	{
		AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, assetId);
		if (MarkNotificationSeen(assetUpdatedNotificationKey))
		{
			notificationCount--;
			if (broadcastCountChanged)
			{
				BroadcastNotificationCountUpdated();
			}
			BroadcastAssetStatus(assetUpdatedNotificationKey, UpdateStatus.SeenUpdate);
		}
	}

	public void MarkAssetVersionAsLatest(SerializableGuid gameId, SerializableGuid assetId)
	{
		AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, assetId);
		AssetUpdatedNotificationStatus notification = GetNotification(assetUpdatedNotificationKey);
		if (RemoveNotification(assetUpdatedNotificationKey))
		{
			if (notification != null && notification.Status == NotificationState.New)
			{
				notificationCount--;
			}
			BroadcastNotificationCountUpdated();
		}
		BroadcastAssetStatus(assetUpdatedNotificationKey, UpdateStatus.UpToDate);
	}

	public void MarkAllSeen(Game game)
	{
		List<AssetReference> list = game.GatherDependentAssetReferences();
		int num = notificationCount;
		foreach (AssetReference item in list)
		{
			SetAssetUpdateSeen(game.AssetID, item.AssetID, broadcastCountChanged: false);
		}
		if (notificationCount != num)
		{
			BroadcastNotificationCountUpdated();
		}
	}

	private void BroadcastNotificationCountUpdated()
	{
		Debug.Log($"notification: I found {notificationCount} assets that could be updated!");
		OnNumberOfNotificationsChanged.Invoke(notificationCount);
	}

	public void SaveAndCleanup()
	{
		Save("GameEditorAssetVersionNotifications-" + activeGameId);
		notificationCount = 0;
		ClearNotifications();
	}
}
