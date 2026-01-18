using System;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003F7 RID: 1015
	public class UIFriendRequestListModel : UIBasePaginatedSocialListModel<FriendRequest>
	{
		// Token: 0x1700052A RID: 1322
		// (get) Token: 0x06001959 RID: 6489 RVA: 0x00074E07 File Offset: 0x00073007
		protected override Task<GraphQlResult> RequestListTask
		{
			get
			{
				return EndlessServices.Instance.CloudService.GetFriendRequestsAsync(base.PaginationParams, false);
			}
		}

		// Token: 0x1700052B RID: 1323
		// (get) Token: 0x0600195A RID: 6490 RVA: 0x00074E1F File Offset: 0x0007301F
		protected override ErrorCodes GetDataErrorCode
		{
			get
			{
				return ErrorCodes.UIFriendRequestList_RequestListTask;
			}
		}

		// Token: 0x0600195B RID: 6491 RVA: 0x00074E28 File Offset: 0x00073028
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
