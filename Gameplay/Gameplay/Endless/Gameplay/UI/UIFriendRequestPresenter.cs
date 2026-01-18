using System;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003F8 RID: 1016
	public class UIFriendRequestPresenter : UIBasePresenter<FriendRequest>
	{
		// Token: 0x0600195D RID: 6493 RVA: 0x00074ED0 File Offset: 0x000730D0
		protected override void Start()
		{
			base.Start();
			this.typedView = base.View.Interface as UIFriendRequestView;
			this.typedView.AcceptFriendRequest += this.AcceptFriendRequest;
			this.typedView.RejectFriendRequest += this.RejectFriendRequest;
			this.typedView.CancelFriendRequest += this.CancelFriendRequest;
		}

		// Token: 0x0600195E RID: 6494 RVA: 0x00074F40 File Offset: 0x00073140
		private void OnDestroy()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			this.typedView.AcceptFriendRequest -= this.AcceptFriendRequest;
			this.typedView.RejectFriendRequest -= this.RejectFriendRequest;
			this.typedView.CancelFriendRequest -= this.CancelFriendRequest;
		}

		// Token: 0x14000038 RID: 56
		// (add) Token: 0x0600195F RID: 6495 RVA: 0x00074FAC File Offset: 0x000731AC
		// (remove) Token: 0x06001960 RID: 6496 RVA: 0x00074FE0 File Offset: 0x000731E0
		public static event Action<FriendRequest> OnAcceptFriendRequest;

		// Token: 0x14000039 RID: 57
		// (add) Token: 0x06001961 RID: 6497 RVA: 0x00075014 File Offset: 0x00073214
		// (remove) Token: 0x06001962 RID: 6498 RVA: 0x00075048 File Offset: 0x00073248
		public static event Action<FriendRequest> OnRejectFriendRequest;

		// Token: 0x1400003A RID: 58
		// (add) Token: 0x06001963 RID: 6499 RVA: 0x0007507C File Offset: 0x0007327C
		// (remove) Token: 0x06001964 RID: 6500 RVA: 0x000750B0 File Offset: 0x000732B0
		public static event Action<FriendRequest> OnCancelFriendRequest;

		// Token: 0x06001965 RID: 6501 RVA: 0x000750E4 File Offset: 0x000732E4
		private void AcceptFriendRequest()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "AcceptFriendRequest", Array.Empty<object>());
			}
			EndlessServices.Instance.CloudService.AcceptFriendRequestAsync(base.Model.RequestId, base.VerboseLogging);
			Action<FriendRequest> onAcceptFriendRequest = UIFriendRequestPresenter.OnAcceptFriendRequest;
			if (onAcceptFriendRequest == null)
			{
				return;
			}
			onAcceptFriendRequest(base.Model);
		}

		// Token: 0x06001966 RID: 6502 RVA: 0x00075140 File Offset: 0x00073340
		private void RejectFriendRequest()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "RejectFriendRequest", Array.Empty<object>());
			}
			EndlessServices.Instance.CloudService.RejectFriendRequestAsync(base.Model.RequestId, base.VerboseLogging);
			Action<FriendRequest> onRejectFriendRequest = UIFriendRequestPresenter.OnRejectFriendRequest;
			if (onRejectFriendRequest == null)
			{
				return;
			}
			onRejectFriendRequest(base.Model);
		}

		// Token: 0x06001967 RID: 6503 RVA: 0x0007519C File Offset: 0x0007339C
		private void CancelFriendRequest()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "CancelFriendRequest", Array.Empty<object>());
			}
			EndlessServices.Instance.CloudService.CancelFriendRequestAsync(base.Model.RequestId, base.VerboseLogging);
			Action<FriendRequest> onCancelFriendRequest = UIFriendRequestPresenter.OnCancelFriendRequest;
			if (onCancelFriendRequest == null)
			{
				return;
			}
			onCancelFriendRequest(base.Model);
		}

		// Token: 0x0400143C RID: 5180
		private UIFriendRequestView typedView;
	}
}
