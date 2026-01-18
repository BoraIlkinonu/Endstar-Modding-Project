using System;
using System.Collections.Generic;
using Endless.Matchmaking;
using Endless.Networking;
using Endless.Shared.Debugging;
using Endless.Shared.SoVariables;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Core.UI
{
	// Token: 0x020000A0 RID: 160
	public class UIUserGroupView : UIGameObject
	{
		// Token: 0x17000057 RID: 87
		// (get) Token: 0x06000361 RID: 865 RVA: 0x000082D0 File Offset: 0x000064D0
		private GroupInfo UserGroup
		{
			get
			{
				return MatchmakingClientController.Instance.LocalGroup;
			}
		}

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000362 RID: 866 RVA: 0x00011EAC File Offset: 0x000100AC
		private bool IsInUserGroup
		{
			get
			{
				return this.UserGroup != null;
			}
		}

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000363 RID: 867 RVA: 0x00011EB8 File Offset: 0x000100B8
		private ClientData LocalClientData
		{
			get
			{
				return MatchmakingClientController.Instance.LocalClientData.Value;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000364 RID: 868 RVA: 0x00011ED7 File Offset: 0x000100D7
		private bool LocalClientIsGroupHost
		{
			get
			{
				return this.IsInUserGroup && this.UserGroup.Host == this.LocalClientData.CoreData;
			}
		}

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x06000365 RID: 869 RVA: 0x00011EFE File Offset: 0x000100FE
		private List<CoreClientData> MembersWithoutGroupHost
		{
			get
			{
				List<CoreClientData> list = new List<CoreClientData>(this.UserGroup.Members);
				list.Remove(this.UserGroup.Host);
				return list;
			}
		}

		// Token: 0x06000366 RID: 870 RVA: 0x00011F24 File Offset: 0x00010124
		public void Initialize(UISocialView socialView)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Initialize", new object[] { socialView.name });
			}
			MatchmakingClientController.GroupJoined += this.OnGroupJoined;
			MatchmakingClientController.GroupJoin += this.OnUserGroupJoin;
			MatchmakingClientController.GroupLeave += this.OnUserGroupLeave;
			MatchmakingClientController.GroupLeft += this.OnGroupLeft;
			MatchmakingClientController.GroupHostChanged += this.OnUserGroupLeaderChange;
		}

		// Token: 0x06000367 RID: 871 RVA: 0x00011FA8 File Offset: 0x000101A8
		public void View()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "View", Array.Empty<object>());
			}
			this.userGroupTitleText.text = string.Format("Party {0}/{1}", this.IsInUserGroup ? this.UserGroup.Members.Count : 1, this.userGroupMaxSize.Value);
			this.hostClientDataView.Display(this.IsInUserGroup ? this.UserGroup.Host : this.LocalClientData.CoreData);
			this.clientDataListModel.SetCanRemove(this.LocalClientIsGroupHost);
			List<ClientData> list = new List<ClientData>();
			if (this.IsInUserGroup)
			{
				List<CoreClientData> list2 = new List<CoreClientData>(this.UserGroup.Members);
				list2.Remove(this.UserGroup.Host);
				foreach (CoreClientData coreClientData in list2)
				{
					ClientData clientData = new ClientData(coreClientData.PlatformId, coreClientData.Platform, "[userName]");
					list.Add(clientData);
				}
			}
			this.clientDataListModel.Set(list, true);
			bool flag = false;
			if (this.IsInUserGroup)
			{
				flag = this.UserGroup.Members.Count > 1;
			}
			this.leaveGroupButton.gameObject.SetActive(flag);
			LayoutRebuilder.MarkLayoutForRebuild(base.RectTransform);
		}

		// Token: 0x06000368 RID: 872 RVA: 0x00012120 File Offset: 0x00010320
		private void OnGroupJoined()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGroupJoined", Array.Empty<object>());
			}
			this.View();
		}

		// Token: 0x06000369 RID: 873 RVA: 0x00012140 File Offset: 0x00010340
		private void OnUserGroupJoin(string userId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupJoin", new object[] { userId });
			}
			this.View();
		}

		// Token: 0x0600036A RID: 874 RVA: 0x00012165 File Offset: 0x00010365
		private void OnUserGroupLeave(string userId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupLeave", new object[] { userId });
			}
			this.View();
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0001218A File Offset: 0x0001038A
		private void OnGroupLeft()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnGroupLeft", Array.Empty<object>());
			}
			this.View();
		}

		// Token: 0x0600036C RID: 876 RVA: 0x000121AA File Offset: 0x000103AA
		private void OnUserGroupLeaderChange(string newHostId)
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "OnUserGroupLeaderChange", new object[] { newHostId });
			}
			this.View();
		}

		// Token: 0x04000278 RID: 632
		[SerializeField]
		private IntVariable userGroupMaxSize;

		// Token: 0x04000279 RID: 633
		[SerializeField]
		private TextMeshProUGUI userGroupTitleText;

		// Token: 0x0400027A RID: 634
		[SerializeField]
		private UIClientDataView hostClientDataView;

		// Token: 0x0400027B RID: 635
		[SerializeField]
		private UIClientDataListModel clientDataListModel;

		// Token: 0x0400027C RID: 636
		[SerializeField]
		private UIButton leaveGroupButton;

		// Token: 0x0400027D RID: 637
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
