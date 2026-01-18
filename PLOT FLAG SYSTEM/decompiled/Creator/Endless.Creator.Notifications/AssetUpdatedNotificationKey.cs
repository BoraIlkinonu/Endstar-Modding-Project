using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Creator.Notifications;

[Serializable]
public class AssetUpdatedNotificationKey : NotificationKey<AssetUpdatedNotificationStatus>
{
	[JsonProperty]
	private SerializableGuid gameId;

	[JsonProperty]
	private SerializableGuid assetId;

	public AssetUpdatedNotificationKey(SerializableGuid gameId, SerializableGuid assetId)
	{
		this.gameId = gameId;
		this.assetId = assetId;
	}

	protected override bool IsNewer(AssetUpdatedNotificationStatus oldStatus, AssetUpdatedNotificationStatus newStatus)
	{
		return oldStatus.Version < newStatus.Version;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((AssetUpdatedNotificationKey)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(gameId, assetId);
	}

	private bool Equals(AssetUpdatedNotificationKey other)
	{
		if (gameId.Equals(other.gameId))
		{
			return assetId.Equals(other.assetId);
		}
		return false;
	}
}
