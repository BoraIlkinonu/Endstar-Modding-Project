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

namespace Endless.Creator
{
	// Token: 0x0200007D RID: 125
	public class GameEditorAssetVersionManager : NotificationManager<GameEditorAssetVersionManager, AssetUpdatedNotificationKey, AssetUpdatedNotificationStatus>
	{
		// Token: 0x060001B0 RID: 432 RVA: 0x0000D2E4 File Offset: 0x0000B4E4
		public async void LoadAssetUpdateStates(Game game, CancellationToken cancelToken)
		{
			this.activeGameId = game.AssetID;
			this.notificationCount = 0;
			base.Load("GameEditorAssetVersionNotifications-" + this.activeGameId);
			BulkAssetCacheResult<Prop> bulkAssetCacheResult = await EndlessAssetCache.GetBulkAssetsAsync<Prop>(game.GameLibrary.PropReferences.Select((AssetReference reference) => new ValueTuple<SerializableGuid, string>(reference.AssetID, "")).ToArray<ValueTuple<SerializableGuid, string>>());
			BulkAssetCacheResult<Prop> bulkPropResult = bulkAssetCacheResult;
			if (!cancelToken.IsCancellationRequested)
			{
				SerializableGuid gameAssetID = game.AssetID;
				using (List<Prop>.Enumerator enumerator = bulkPropResult.Assets.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Prop asset2 = enumerator.Current;
						if (game.GameLibrary.PropReferences.First((AssetReference reference) => reference.AssetID == asset2.AssetID).AssetVersion != asset2.AssetVersion)
						{
							this.ProcessAssetReferenceVersion(gameAssetID, asset2);
						}
					}
				}
				List<AssetReference> activeTerrainReferences = game.GetActiveTerrainReferences();
				BulkAssetCacheResult<TerrainTilesetCosmeticAsset> bulkAssetCacheResult2 = await EndlessAssetCache.GetBulkAssetsAsync<TerrainTilesetCosmeticAsset>(activeTerrainReferences.Select((AssetReference reference) => new ValueTuple<SerializableGuid, string>(reference.AssetID, "")).ToArray<ValueTuple<SerializableGuid, string>>());
				if (!cancelToken.IsCancellationRequested)
				{
					using (List<TerrainTilesetCosmeticAsset>.Enumerator enumerator2 = bulkAssetCacheResult2.Assets.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							TerrainTilesetCosmeticAsset asset = enumerator2.Current;
							if (activeTerrainReferences.First((AssetReference reference) => reference.AssetID == asset.AssetID).AssetVersion != asset.AssetVersion)
							{
								this.ProcessAssetReferenceVersion(gameAssetID, asset);
							}
						}
					}
					if (bulkPropResult.HasErrors)
					{
						Debug.LogException(new NotImplementedException("We have no error handling here!", bulkPropResult.GetErrorMessage()));
					}
					this.BroadcastNotificationCountUpdated();
				}
			}
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x0000D32C File Offset: 0x0000B52C
		private void ProcessAssetReferenceVersion(SerializableGuid gameId, Asset prop)
		{
			AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, prop.AssetID);
			AssetUpdatedNotificationStatus assetUpdatedNotificationStatus = base.AddOrUpdateNotification(assetUpdatedNotificationKey, new AssetUpdatedNotificationStatus(SemanticVersion.Parse(prop.AssetVersion)));
			if (assetUpdatedNotificationStatus.Status == NotificationState.New)
			{
				this.notificationCount++;
				this.BroadcastAssetStatus(assetUpdatedNotificationKey, GameEditorAssetVersionManager.UpdateStatus.NewUpdate);
				return;
			}
			if (assetUpdatedNotificationStatus.Status == NotificationState.Seen)
			{
				this.BroadcastAssetStatus(assetUpdatedNotificationKey, GameEditorAssetVersionManager.UpdateStatus.SeenUpdate);
			}
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x0000D394 File Offset: 0x0000B594
		private void BroadcastAssetStatus(AssetUpdatedNotificationKey notificationKey, GameEditorAssetVersionManager.UpdateStatus statusToBroadcast)
		{
			List<Action<GameEditorAssetVersionManager.UpdateStatus>> list;
			if (this.callbackMap.TryGetValue(notificationKey, out list))
			{
				foreach (Action<GameEditorAssetVersionManager.UpdateStatus> action in list)
				{
					try
					{
						action(statusToBroadcast);
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
					}
				}
			}
		}

		// Token: 0x060001B3 RID: 435 RVA: 0x0000D408 File Offset: 0x0000B608
		public int GetNumberOfNotifications(Game game)
		{
			return this.notificationCount;
		}

		// Token: 0x060001B4 RID: 436 RVA: 0x0000D410 File Offset: 0x0000B610
		public void SubscribeToAssetVersionUpdated(SerializableGuid gameId, SerializableGuid assetId, Action<GameEditorAssetVersionManager.UpdateStatus> callback)
		{
			AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, assetId);
			if (!this.callbackMap.ContainsKey(assetUpdatedNotificationKey))
			{
				this.callbackMap.Add(assetUpdatedNotificationKey, new List<Action<GameEditorAssetVersionManager.UpdateStatus>>());
			}
			this.callbackMap[assetUpdatedNotificationKey].Add(callback);
			if (base.HasNotification(assetUpdatedNotificationKey))
			{
				AssetUpdatedNotificationStatus notification = base.GetNotification(assetUpdatedNotificationKey);
				if (notification.Status == NotificationState.New)
				{
					callback(GameEditorAssetVersionManager.UpdateStatus.NewUpdate);
					return;
				}
				if (notification.Status == NotificationState.Seen)
				{
					callback(GameEditorAssetVersionManager.UpdateStatus.SeenUpdate);
					return;
				}
			}
			else
			{
				callback(GameEditorAssetVersionManager.UpdateStatus.UpToDate);
			}
		}

		// Token: 0x060001B5 RID: 437 RVA: 0x0000D490 File Offset: 0x0000B690
		public void UnsubscribeToAssetVersionUpdated(SerializableGuid gameId, SerializableGuid assetId, Action<GameEditorAssetVersionManager.UpdateStatus> callback)
		{
			AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, assetId);
			if (this.callbackMap.ContainsKey(assetUpdatedNotificationKey))
			{
				this.callbackMap[assetUpdatedNotificationKey].Remove(callback);
				if (this.callbackMap[assetUpdatedNotificationKey].Count == 0)
				{
					this.callbackMap.Remove(assetUpdatedNotificationKey);
				}
			}
		}

		// Token: 0x060001B6 RID: 438 RVA: 0x0000D4E6 File Offset: 0x0000B6E6
		public void SetAssetUpdateSeen(SerializableGuid gameId, SerializableGuid assetId)
		{
			this.SetAssetUpdateSeen(gameId, assetId, true);
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x0000D4F4 File Offset: 0x0000B6F4
		private void SetAssetUpdateSeen(SerializableGuid gameId, SerializableGuid assetId, bool broadcastCountChanged)
		{
			AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, assetId);
			if (base.MarkNotificationSeen(assetUpdatedNotificationKey))
			{
				this.notificationCount--;
				if (broadcastCountChanged)
				{
					this.BroadcastNotificationCountUpdated();
				}
				this.BroadcastAssetStatus(assetUpdatedNotificationKey, GameEditorAssetVersionManager.UpdateStatus.SeenUpdate);
			}
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x0000D534 File Offset: 0x0000B734
		public void MarkAssetVersionAsLatest(SerializableGuid gameId, SerializableGuid assetId)
		{
			AssetUpdatedNotificationKey assetUpdatedNotificationKey = new AssetUpdatedNotificationKey(gameId, assetId);
			AssetUpdatedNotificationStatus notification = base.GetNotification(assetUpdatedNotificationKey);
			if (base.RemoveNotification(assetUpdatedNotificationKey))
			{
				if (notification != null && notification.Status == NotificationState.New)
				{
					this.notificationCount--;
				}
				this.BroadcastNotificationCountUpdated();
			}
			this.BroadcastAssetStatus(assetUpdatedNotificationKey, GameEditorAssetVersionManager.UpdateStatus.UpToDate);
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0000D584 File Offset: 0x0000B784
		public void MarkAllSeen(Game game)
		{
			List<AssetReference> list = game.GatherDependentAssetReferences();
			int num = this.notificationCount;
			foreach (AssetReference assetReference in list)
			{
				this.SetAssetUpdateSeen(game.AssetID, assetReference.AssetID, false);
			}
			if (this.notificationCount != num)
			{
				this.BroadcastNotificationCountUpdated();
			}
		}

		// Token: 0x060001BA RID: 442 RVA: 0x0000D604 File Offset: 0x0000B804
		private void BroadcastNotificationCountUpdated()
		{
			Debug.Log(string.Format("notification: I found {0} assets that could be updated!", this.notificationCount));
			this.OnNumberOfNotificationsChanged.Invoke(this.notificationCount);
		}

		// Token: 0x060001BB RID: 443 RVA: 0x0000D631 File Offset: 0x0000B831
		public void SaveAndCleanup()
		{
			base.Save("GameEditorAssetVersionNotifications-" + this.activeGameId);
			this.notificationCount = 0;
			base.ClearNotifications();
		}

		// Token: 0x0400022E RID: 558
		private const string BASE_FILE_NAME = "GameEditorAssetVersionNotifications";

		// Token: 0x0400022F RID: 559
		[FormerlySerializedAs("OnNumberOfUpdatesChanged")]
		[HideInInspector]
		public UnityEvent<int> OnNumberOfNotificationsChanged = new UnityEvent<int>();

		// Token: 0x04000230 RID: 560
		private int notificationCount;

		// Token: 0x04000231 RID: 561
		private Dictionary<AssetUpdatedNotificationKey, List<Action<GameEditorAssetVersionManager.UpdateStatus>>> callbackMap = new Dictionary<AssetUpdatedNotificationKey, List<Action<GameEditorAssetVersionManager.UpdateStatus>>>();

		// Token: 0x04000232 RID: 562
		private string activeGameId;

		// Token: 0x0200007E RID: 126
		public enum UpdateStatus
		{
			// Token: 0x04000234 RID: 564
			UpToDate,
			// Token: 0x04000235 RID: 565
			SeenUpdate,
			// Token: 0x04000236 RID: 566
			NewUpdate
		}
	}
}
