using System;
using Endless.Shared.DataTypes;
using Newtonsoft.Json;

namespace Endless.Shared.Social
{
	// Token: 0x020000E2 RID: 226
	[Serializable]
	public class FriendRequest
	{
		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x06000587 RID: 1415 RVA: 0x00018063 File Offset: 0x00016263
		// (set) Token: 0x06000588 RID: 1416 RVA: 0x0001806B File Offset: 0x0001626B
		[JsonProperty("id")]
		public string RequestId { get; private set; }

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000589 RID: 1417 RVA: 0x00018074 File Offset: 0x00016274
		// (set) Token: 0x0600058A RID: 1418 RVA: 0x0001807C File Offset: 0x0001627C
		[JsonProperty("recipient")]
		public User Recipient { get; private set; }

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x0600058B RID: 1419 RVA: 0x00018085 File Offset: 0x00016285
		// (set) Token: 0x0600058C RID: 1420 RVA: 0x0001808D File Offset: 0x0001628D
		[JsonProperty("sender")]
		public User Sender { get; private set; }

		// Token: 0x170000DC RID: 220
		// (get) Token: 0x0600058D RID: 1421 RVA: 0x00018096 File Offset: 0x00016296
		// (set) Token: 0x0600058E RID: 1422 RVA: 0x0001809E File Offset: 0x0001629E
		[JsonProperty("request_status")]
		public string RequestStatus { get; private set; }

		// Token: 0x0600058F RID: 1423 RVA: 0x000180A7 File Offset: 0x000162A7
		public bool IsPending()
		{
			return this.RequestStatus == "pending";
		}

		// Token: 0x06000590 RID: 1424 RVA: 0x000180BC File Offset: 0x000162BC
		public override string ToString()
		{
			return string.Format("{{ {0}: {1}, {2}: {3}, {4}: {5}, {6}: {7} }}", new object[] { "RequestId", this.RequestId, "Recipient", this.Recipient, "Sender", this.Sender, "RequestStatus", this.RequestStatus });
		}

		// Token: 0x040002E7 RID: 743
		public const string FRIEND_REQUEST_STATUS_ACCEPTED = "accepted";

		// Token: 0x040002E8 RID: 744
		public const string FRIEND_REQUEST_STATUS_PENDING = "pending";

		// Token: 0x040002E9 RID: 745
		public const string FRIEND_REQUEST_STATUS_REJECTED = "rejected";
	}
}
