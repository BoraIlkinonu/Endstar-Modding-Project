using Endless.Shared.DataTypes;

namespace Endless.Creator.Notifications;

public class AssetUpdatedNotificationStatus : NotificationStatus
{
	public SemanticVersion Version { get; set; }

	public AssetUpdatedNotificationStatus(SemanticVersion version)
	{
		Version = version;
	}
}
