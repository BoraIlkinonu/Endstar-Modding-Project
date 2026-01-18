using System;
using Endless.Matchmaking;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Core.UI
{
	// Token: 0x0200009F RID: 159
	public class UIUserGroupController : UIGameObject
	{
		// Token: 0x0600035E RID: 862 RVA: 0x00011E50 File Offset: 0x00010050
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.leaveGroupButton.onClick.AddListener(new UnityAction(this.LeaveGroup));
		}

		// Token: 0x0600035F RID: 863 RVA: 0x00011E86 File Offset: 0x00010086
		private void LeaveGroup()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "LeaveGroup", Array.Empty<object>());
			}
			MatchmakingClientController.Instance.LeaveGroup(true, null);
		}

		// Token: 0x04000275 RID: 629
		[SerializeField]
		private UIUserGroupView view;

		// Token: 0x04000276 RID: 630
		[SerializeField]
		private UIButton leaveGroupButton;

		// Token: 0x04000277 RID: 631
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
