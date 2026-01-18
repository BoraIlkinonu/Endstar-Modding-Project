using System;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.Social;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Gameplay.UI
{
	// Token: 0x020003FA RID: 1018
	public class UIFriendRequestView : UIBaseSocialView<FriendRequest>
	{
		// Token: 0x06001969 RID: 6505 RVA: 0x00075200 File Offset: 0x00073400
		private void Start()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.acceptFriendRequestButton.onClick.AddListener(new UnityAction(this.InvokeAcceptFriendRequest));
			this.rejectFriendRequestButton.onClick.AddListener(new UnityAction(this.InvokeRejectFriendRequest));
			this.cancelFriendRequestButton.onClick.AddListener(new UnityAction(this.InvokeCancelFriendRequest));
		}

		// Token: 0x1400003B RID: 59
		// (add) Token: 0x0600196A RID: 6506 RVA: 0x0007527C File Offset: 0x0007347C
		// (remove) Token: 0x0600196B RID: 6507 RVA: 0x000752B4 File Offset: 0x000734B4
		public event Action AcceptFriendRequest;

		// Token: 0x1400003C RID: 60
		// (add) Token: 0x0600196C RID: 6508 RVA: 0x000752EC File Offset: 0x000734EC
		// (remove) Token: 0x0600196D RID: 6509 RVA: 0x00075324 File Offset: 0x00073524
		public event Action RejectFriendRequest;

		// Token: 0x1400003D RID: 61
		// (add) Token: 0x0600196E RID: 6510 RVA: 0x0007535C File Offset: 0x0007355C
		// (remove) Token: 0x0600196F RID: 6511 RVA: 0x00075394 File Offset: 0x00073594
		public event Action CancelFriendRequest;

		// Token: 0x06001970 RID: 6512 RVA: 0x000753CC File Offset: 0x000735CC
		public override void View(FriendRequest model)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "View", new object[] { model });
			}
			User user = UISocialUtility.ExtractNonActiveUser(model);
			this.userView.View(user);
			UIFriendRequestType uifriendRequestType = ((model.Sender.Id == EndlessServices.Instance.CloudService.ActiveUserId) ? UIFriendRequestType.Sent : UIFriendRequestType.Received);
			this.userNameRectTransformDictionary.Apply(uifriendRequestType.ToString());
			this.acceptFriendRequestButton.gameObject.SetActive(uifriendRequestType == UIFriendRequestType.Received);
			this.rejectFriendRequestButton.gameObject.SetActive(uifriendRequestType == UIFriendRequestType.Received);
			this.cancelFriendRequestButton.gameObject.SetActive(uifriendRequestType == UIFriendRequestType.Sent);
		}

		// Token: 0x06001971 RID: 6513 RVA: 0x00075480 File Offset: 0x00073680
		private void InvokeAcceptFriendRequest()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeAcceptFriendRequest", Array.Empty<object>());
			}
			Action acceptFriendRequest = this.AcceptFriendRequest;
			if (acceptFriendRequest == null)
			{
				return;
			}
			acceptFriendRequest();
		}

		// Token: 0x06001972 RID: 6514 RVA: 0x000754AA File Offset: 0x000736AA
		private void InvokeRejectFriendRequest()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeRejectFriendRequest", Array.Empty<object>());
			}
			Action rejectFriendRequest = this.RejectFriendRequest;
			if (rejectFriendRequest == null)
			{
				return;
			}
			rejectFriendRequest();
		}

		// Token: 0x06001973 RID: 6515 RVA: 0x000754D4 File Offset: 0x000736D4
		private void InvokeCancelFriendRequest()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "InvokeCancelFriendRequest", Array.Empty<object>());
			}
			Action cancelFriendRequest = this.CancelFriendRequest;
			if (cancelFriendRequest == null)
			{
				return;
			}
			cancelFriendRequest();
		}

		// Token: 0x04001443 RID: 5187
		[Header("UIFriendRequestView")]
		[SerializeField]
		private UIUserView userView;

		// Token: 0x04001444 RID: 5188
		[SerializeField]
		private UIRectTransformDictionary userNameRectTransformDictionary;

		// Token: 0x04001445 RID: 5189
		[SerializeField]
		private UIButton acceptFriendRequestButton;

		// Token: 0x04001446 RID: 5190
		[SerializeField]
		private UIButton rejectFriendRequestButton;

		// Token: 0x04001447 RID: 5191
		[SerializeField]
		private UIButton cancelFriendRequestButton;
	}
}
