using System;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003F4 RID: 1012
	public class UIBlockedUserListModel : UIBasePaginatedSocialListModel<BlockedUser>
	{
		// Token: 0x17000528 RID: 1320
		// (get) Token: 0x0600194B RID: 6475 RVA: 0x00074B60 File Offset: 0x00072D60
		protected override Task<GraphQlResult> RequestListTask
		{
			get
			{
				return EndlessServices.Instance.CloudService.GetBlockedUsersAsync(base.PaginationParams, false);
			}
		}

		// Token: 0x17000529 RID: 1321
		// (get) Token: 0x0600194C RID: 6476 RVA: 0x00074B78 File Offset: 0x00072D78
		protected override ErrorCodes GetDataErrorCode
		{
			get
			{
				return ErrorCodes.UIBlockedUserList_RequestListTask;
			}
		}

		// Token: 0x0600194D RID: 6477 RVA: 0x00074B80 File Offset: 0x00072D80
		public override bool RemoveItemWithId(int itemId)
		{
			if (this.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RemoveItemWithId", new object[] { itemId });
			}
			for (int i = 0; i < this.items.Count; i++)
			{
				if (UISocialUtility.ExtractNonActiveUser(this.items[i]).Id == itemId)
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
