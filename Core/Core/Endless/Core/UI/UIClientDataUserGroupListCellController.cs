using System;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x02000056 RID: 86
	[RequireComponent(typeof(UIClientDataListCellView))]
	public class UIClientDataUserGroupListCellController : UIGameObject
	{
		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000197 RID: 407 RVA: 0x0000A0D9 File Offset: 0x000082D9
		private UIClientDataListModel ClientDataListModel
		{
			get
			{
				return (UIClientDataListModel)this.view.ListView.Model;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000198 RID: 408 RVA: 0x0000A0F0 File Offset: 0x000082F0
		private ClientData Entry
		{
			get
			{
				return this.ClientDataListModel[this.view.DataIndex];
			}
		}

		// Token: 0x06000199 RID: 409 RVA: 0x0000A108 File Offset: 0x00008308
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.removeButton.onClick.AddListener(new UnityAction(this.Remove));
			base.TryGetComponent<UIClientDataListCellView>(out this.view);
		}

		// Token: 0x0600019A RID: 410 RVA: 0x0000A158 File Offset: 0x00008358
		private void Remove()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Remove", Array.Empty<object>());
			}
			MatchmakingClientController.Instance.RemoveFromGroup(this.Entry.CoreData.PlatformId, null);
			this.ClientDataListModel.RemoveAt(this.view.DataIndex, true);
		}

		// Token: 0x04000122 RID: 290
		[SerializeField]
		private UIButton removeButton;

		// Token: 0x04000123 RID: 291
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;

		// Token: 0x04000124 RID: 292
		private UIClientDataListCellView view;
	}
}
