using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.Tweens;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200004A RID: 74
	[RequireComponent(typeof(UIDisplayAndHideHandler))]
	public class UIInviteNotificationView : UIGameObject, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600016B RID: 363 RVA: 0x00009323 File Offset: 0x00007523
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600016C RID: 364 RVA: 0x0000932B File Offset: 0x0000752B
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600016D RID: 365 RVA: 0x00009333 File Offset: 0x00007533
		// (set) Token: 0x0600016E RID: 366 RVA: 0x0000933B File Offset: 0x0000753B
		public UIInviteNotification DisplayedInviteNotification { get; private set; }

		// Token: 0x0600016F RID: 367 RVA: 0x00009344 File Offset: 0x00007544
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			base.TryGetComponent<UIDisplayAndHideHandler>(out this.displayAndHideHandler);
			this.displayAndHideHandler.OnHideComplete.AddListener(new UnityAction(this.OnHideComplete));
			this.displayAndHideHandler.SetToHideEnd(true);
			MatchmakingClientController.GroupInviteReceived += this.OnUserGroupInvite;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x000093AF File Offset: 0x000075AF
		private void OnDestroy()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnDestroy", Array.Empty<object>());
			}
			ValueTween.CancelAndNull(ref this.expirationTween);
		}

		// Token: 0x06000171 RID: 369 RVA: 0x000093D4 File Offset: 0x000075D4
		public void Display()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Display", Array.Empty<object>());
			}
			this.displayAndHideHandler.Display();
			this.DisplayedInviteNotification = this.inviteNotifications.Dequeue();
			ValueTween.CancelAndNull(ref this.expirationTween);
			TimeSpan timeSpan = DateTime.Now.AddSeconds((double)this.expiration.Value) - DateTime.Now;
			this.expirationTween = MonoBehaviourSingleton<TweenManager>.Instance.TweenValue(1f, 0f, (float)timeSpan.TotalSeconds, new Action<float>(this.UpdateSlider), delegate
			{
				this.Hide();
			}, TweenTimeMode.Scaled, null);
			UIInviteNotification.UIInviteNotificationTypes inviteNotificationType = this.DisplayedInviteNotification.InviteNotificationType;
			if (inviteNotificationType == UIInviteNotification.UIInviteNotificationTypes.UserGroupInvite)
			{
				this.inviteText.text = this.DisplayedInviteNotification.InviterName + " is inviting you to their group.";
				return;
			}
			if (inviteNotificationType != UIInviteNotification.UIInviteNotificationTypes.MatchInvite)
			{
				DebugUtility.LogWarning(this, "Display", string.Format("No support for a {0} value of {1}!", "UIInviteNotificationTypes", this.DisplayedInviteNotification.InviteNotificationType), Array.Empty<object>());
				return;
			}
			this.inviteText.text = this.DisplayedInviteNotification.InviterName + " is inviting you to their match.";
		}

		// Token: 0x06000172 RID: 370 RVA: 0x00009508 File Offset: 0x00007708
		public void Hide()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Hide", Array.Empty<object>());
			}
			this.displayAndHideHandler.Hide();
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.viewUserGroupInviteAsyncCancellationTokenSource);
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.viewMatchInviteAsyncCancellationTokenSource);
			ValueTween.CancelAndNull(ref this.expirationTween);
		}

		// Token: 0x06000173 RID: 371 RVA: 0x00009559 File Offset: 0x00007759
		private void OnHideComplete()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnHideComplete", Array.Empty<object>());
			}
			if (this.inviteNotifications.Count > 0)
			{
				this.Display();
			}
		}

		// Token: 0x06000174 RID: 372 RVA: 0x00009588 File Offset: 0x00007788
		private void OnUserGroupInvite(string groupId, string inviteUserId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.Log(string.Concat(new string[] { "OnUserGroupInvite ( groupId: ", groupId, ", inviteUserId: ", inviteUserId, " )" }), this);
			}
			this.ViewUserGroupInviteAsync(groupId, inviteUserId);
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000095D8 File Offset: 0x000077D8
		private Task ViewUserGroupInviteAsync(string groupId, string inviteUserId)
		{
			UIInviteNotificationView.<ViewUserGroupInviteAsync>d__26 <ViewUserGroupInviteAsync>d__;
			<ViewUserGroupInviteAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<ViewUserGroupInviteAsync>d__.<>4__this = this;
			<ViewUserGroupInviteAsync>d__.groupId = groupId;
			<ViewUserGroupInviteAsync>d__.inviteUserId = inviteUserId;
			<ViewUserGroupInviteAsync>d__.<>1__state = -1;
			<ViewUserGroupInviteAsync>d__.<>t__builder.Start<UIInviteNotificationView.<ViewUserGroupInviteAsync>d__26>(ref <ViewUserGroupInviteAsync>d__);
			return <ViewUserGroupInviteAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000176 RID: 374 RVA: 0x0000962C File Offset: 0x0000782C
		private Task ViewMatchInviteAsync(MatchData matchData)
		{
			UIInviteNotificationView.<ViewMatchInviteAsync>d__27 <ViewMatchInviteAsync>d__;
			<ViewMatchInviteAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<ViewMatchInviteAsync>d__.<>4__this = this;
			<ViewMatchInviteAsync>d__.matchData = matchData;
			<ViewMatchInviteAsync>d__.<>1__state = -1;
			<ViewMatchInviteAsync>d__.<>t__builder.Start<UIInviteNotificationView.<ViewMatchInviteAsync>d__27>(ref <ViewMatchInviteAsync>d__);
			return <ViewMatchInviteAsync>d__.<>t__builder.Task;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00009677 File Offset: 0x00007877
		private void UpdateSlider(float value)
		{
			if (this.superVerboseLogging)
			{
				DebugUtility.LogMethod(this, "UpdateSlider", new object[] { value });
			}
			this.expirationSlider.value = value;
		}

		// Token: 0x040000F2 RID: 242
		[SerializeField]
		[Tooltip("In Seconds")]
		private FloatVariable expiration;

		// Token: 0x040000F3 RID: 243
		[SerializeField]
		private TextMeshProUGUI inviteText;

		// Token: 0x040000F4 RID: 244
		[SerializeField]
		private UISlider expirationSlider;

		// Token: 0x040000F5 RID: 245
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x040000F6 RID: 246
		[SerializeField]
		private bool superVerboseLogging;

		// Token: 0x040000F7 RID: 247
		private readonly Queue<UIInviteNotification> inviteNotifications = new Queue<UIInviteNotification>();

		// Token: 0x040000F8 RID: 248
		private UIDisplayAndHideHandler displayAndHideHandler;

		// Token: 0x040000F9 RID: 249
		private CancellationTokenSource viewUserGroupInviteAsyncCancellationTokenSource;

		// Token: 0x040000FA RID: 250
		private CancellationTokenSource viewMatchInviteAsyncCancellationTokenSource;

		// Token: 0x040000FB RID: 251
		private ValueTween expirationTween;
	}
}
