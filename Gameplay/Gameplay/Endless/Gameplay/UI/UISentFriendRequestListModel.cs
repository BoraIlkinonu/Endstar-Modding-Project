using System;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003FB RID: 1019
	public class UISentFriendRequestListModel : UIBasePaginatedSocialListModel<FriendRequest>
	{
		// Token: 0x1700052C RID: 1324
		// (get) Token: 0x06001975 RID: 6517 RVA: 0x00075506 File Offset: 0x00073706
		protected override Task<GraphQlResult> RequestListTask
		{
			get
			{
				return EndlessServices.Instance.CloudService.GetSentFriendRequests(base.PaginationParams, false);
			}
		}

		// Token: 0x1700052D RID: 1325
		// (get) Token: 0x06001976 RID: 6518 RVA: 0x0007551E File Offset: 0x0007371E
		protected override ErrorCodes GetDataErrorCode
		{
			get
			{
				return ErrorCodes.UISentFriendRequestList_RequestListTask;
			}
		}

		// Token: 0x06001977 RID: 6519 RVA: 0x00075528 File Offset: 0x00073728
		public override bool RemoveItemWithId(int itemId)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveItemWithId", new object[] { itemId });
			}
			for (int i = 0; i < this.items.Count; i++)
			{
				int num;
				if (int.TryParse(this.items[i].RequestId, out num) && num == itemId)
				{
					this.items.RemoveAt(i);
					if (this.VerboseLogging)
					{
						DebugUtility.LogMethodWithAppension(this, "RemoveItemWithId", "Success", new object[] { itemId });
					}
					base.InvokeOnModelChanged();
					return true;
				}
			}
			return false;
		}
	}
}
