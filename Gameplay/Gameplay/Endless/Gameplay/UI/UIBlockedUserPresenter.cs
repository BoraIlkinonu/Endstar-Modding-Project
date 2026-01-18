using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003F5 RID: 1013
	public class UIBlockedUserPresenter : UIBasePresenter<BlockedUser>
	{
		// Token: 0x0600194F RID: 6479 RVA: 0x00074C22 File Offset: 0x00072E22
		protected override void Start()
		{
			base.Start();
			this.typedView = base.View.Interface as UIBlockedUserView;
			this.typedView.Unblock += this.Unblock;
		}

		// Token: 0x06001950 RID: 6480 RVA: 0x00074C57 File Offset: 0x00072E57
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.typedView.Unblock -= this.Unblock;
		}

		// Token: 0x06001951 RID: 6481 RVA: 0x00074C88 File Offset: 0x00072E88
		private void Unblock()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Unblock", Array.Empty<object>());
			}
			User user = UISocialUtility.ExtractNonActiveUser(base.Model);
			EndlessServices.Instance.CloudService.UnblockUserAsync(user.Id, base.VerboseLogging);
			Action<BlockedUser> onUnblock = UIBlockedUserPresenter.OnUnblock;
			if (onUnblock == null)
			{
				return;
			}
			onUnblock(base.Model);
		}

		// Token: 0x04001437 RID: 5175
		public static Action<BlockedUser> OnUnblock;

		// Token: 0x04001438 RID: 5176
		private UIBlockedUserView typedView;
	}
}
