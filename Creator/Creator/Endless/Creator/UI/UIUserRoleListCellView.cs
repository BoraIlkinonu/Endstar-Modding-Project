using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000192 RID: 402
	public class UIUserRoleListCellView : UIBaseListCellView<UserRole>, IUILoadingSpinnerViewCompatible
	{
		// Token: 0x17000096 RID: 150
		// (get) Token: 0x060005DA RID: 1498 RVA: 0x0001E159 File Offset: 0x0001C359
		public UnityEvent OnLoadingStarted { get; } = new UnityEvent();

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x060005DB RID: 1499 RVA: 0x0001E161 File Offset: 0x0001C361
		public UnityEvent OnLoadingEnded { get; } = new UnityEvent();

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x060005DC RID: 1500 RVA: 0x0001E169 File Offset: 0x0001C369
		// (set) Token: 0x060005DD RID: 1501 RVA: 0x0001E171 File Offset: 0x0001C371
		public string UserName { get; private set; }

		// Token: 0x060005DE RID: 1502 RVA: 0x0001E17A File Offset: 0x0001C37A
		public override void OnDespawn()
		{
			base.OnDespawn();
			this.UserName = string.Empty;
			this.userNameText.enabled = false;
			CancellationTokenSourceUtility.CancelAndCleanup(ref this.initializeCancellationTokenSource);
		}

		// Token: 0x060005DF RID: 1503 RVA: 0x0001E1A4 File Offset: 0x0001C3A4
		public override void View(UIBaseListView<UserRole> listView, int dataIndex)
		{
			base.View(listView, dataIndex);
			if (this.IsAddButton)
			{
				return;
			}
			this.ViewAsync();
		}

		// Token: 0x060005E0 RID: 1504 RVA: 0x0001E1C0 File Offset: 0x0001C3C0
		private Task ViewAsync()
		{
			UIUserRoleListCellView.<ViewAsync>d__17 <ViewAsync>d__;
			<ViewAsync>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<ViewAsync>d__.<>4__this = this;
			<ViewAsync>d__.<>1__state = -1;
			<ViewAsync>d__.<>t__builder.Start<UIUserRoleListCellView.<ViewAsync>d__17>(ref <ViewAsync>d__);
			return <ViewAsync>d__.<>t__builder.Task;
		}

		// Token: 0x060005E1 RID: 1505 RVA: 0x0001E204 File Offset: 0x0001C404
		private bool GetCanLocalClientChangeRole(bool isLocalClient, Roles localClientRole, UIUserRoleListModel userRoleListModel, bool localClientRoleIsGreater)
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "GetCanLocalClientChangeRole", new object[] { isLocalClient, localClientRole, userRoleListModel, localClientRoleIsGreater });
			}
			if (isLocalClient)
			{
				return localClientRole != Roles.Owner || userRoleListModel.OwnerCount > 1;
			}
			return localClientRole.IsGreaterThan(Roles.Viewer) && localClientRoleIsGreater;
		}

		// Token: 0x04000517 RID: 1303
		[Header("UIUserRoleListCellView")]
		[SerializeField]
		private TextMeshProUGUI userNameText;

		// Token: 0x04000518 RID: 1304
		[SerializeField]
		private GameObject inheritedVisual;

		// Token: 0x04000519 RID: 1305
		[SerializeField]
		private TextMeshProUGUI roleText;

		// Token: 0x0400051A RID: 1306
		[SerializeField]
		private UIButton changeRoleButton;

		// Token: 0x0400051B RID: 1307
		private CancellationTokenSource initializeCancellationTokenSource;
	}
}
