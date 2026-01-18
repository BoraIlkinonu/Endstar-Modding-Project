using System;
using System.Threading.Tasks;
using Endless.Data;
using Endless.GraphQl;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003FC RID: 1020
	public class UIFriendshipListModel : UIBasePaginatedSocialListModel<Friendship>
	{
		// Token: 0x1700052E RID: 1326
		// (get) Token: 0x06001979 RID: 6521 RVA: 0x000755C7 File Offset: 0x000737C7
		protected override Task<GraphQlResult> RequestListTask
		{
			get
			{
				return EndlessServices.Instance.CloudService.GetFriendshipsAsync(base.PaginationParams, false);
			}
		}

		// Token: 0x1700052F RID: 1327
		// (get) Token: 0x0600197A RID: 6522 RVA: 0x000755DF File Offset: 0x000737DF
		protected override ErrorCodes GetDataErrorCode
		{
			get
			{
				return ErrorCodes.UIFriendshipList_RequestListTask;
			}
		}

		// Token: 0x0600197B RID: 6523 RVA: 0x000755E8 File Offset: 0x000737E8
		protected override void AddExtractedData(Friendship[] range)
		{
			base.AddExtractedData(range);
			foreach (Friendship friendship in range)
			{
				MonoBehaviourSingleton<RuntimeDatabase>.Instance.CacheUser(friendship.UserOne);
				MonoBehaviourSingleton<RuntimeDatabase>.Instance.CacheUser(friendship.UserTwo);
			}
		}

		// Token: 0x0600197C RID: 6524 RVA: 0x00075630 File Offset: 0x00073830
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
