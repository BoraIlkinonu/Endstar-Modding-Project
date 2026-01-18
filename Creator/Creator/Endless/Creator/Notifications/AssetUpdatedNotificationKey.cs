using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Creator.Notifications
{
	// Token: 0x02000316 RID: 790
	[Serializable]
	public class AssetUpdatedNotificationKey : NotificationKey<AssetUpdatedNotificationStatus>
	{
		// Token: 0x06000E49 RID: 3657 RVA: 0x00043A8E File Offset: 0x00041C8E
		public AssetUpdatedNotificationKey(SerializableGuid gameId, SerializableGuid assetId)
		{
			this.gameId = gameId;
			this.assetId = assetId;
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x00043AA4 File Offset: 0x00041CA4
		protected override bool IsNewer(AssetUpdatedNotificationStatus oldStatus, AssetUpdatedNotificationStatus newStatus)
		{
			return oldStatus.Version < newStatus.Version;
		}

		// Token: 0x06000E4B RID: 3659 RVA: 0x00043AB7 File Offset: 0x00041CB7
		public override bool Equals(object obj)
		{
			return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((AssetUpdatedNotificationKey)obj)));
		}

		// Token: 0x06000E4C RID: 3660 RVA: 0x00043AE5 File Offset: 0x00041CE5
		public override int GetHashCode()
		{
			return HashCode.Combine<SerializableGuid, SerializableGuid>(this.gameId, this.assetId);
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x00043AF8 File Offset: 0x00041CF8
		private bool Equals(AssetUpdatedNotificationKey other)
		{
			return this.gameId.Equals(other.gameId) && this.assetId.Equals(other.assetId);
		}

		// Token: 0x04000C1F RID: 3103
		[JsonProperty]
		private SerializableGuid gameId;

		// Token: 0x04000C20 RID: 3104
		[JsonProperty]
		private SerializableGuid assetId;
	}
}
