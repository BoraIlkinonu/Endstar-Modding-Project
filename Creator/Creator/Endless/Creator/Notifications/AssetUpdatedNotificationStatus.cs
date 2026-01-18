using System;
using Endless.Shared.DataTypes;

namespace Endless.Creator.Notifications
{
	// Token: 0x02000314 RID: 788
	public class AssetUpdatedNotificationStatus : NotificationStatus
	{
		// Token: 0x1700020E RID: 526
		// (get) Token: 0x06000E43 RID: 3651 RVA: 0x00043A53 File Offset: 0x00041C53
		// (set) Token: 0x06000E44 RID: 3652 RVA: 0x00043A5B File Offset: 0x00041C5B
		public SemanticVersion Version { get; set; }

		// Token: 0x06000E45 RID: 3653 RVA: 0x00043A64 File Offset: 0x00041C64
		public AssetUpdatedNotificationStatus(SemanticVersion version)
		{
			this.Version = version;
		}
	}
}
