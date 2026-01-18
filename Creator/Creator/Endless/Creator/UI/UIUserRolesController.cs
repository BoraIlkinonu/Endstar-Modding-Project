using System;
using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x020002B1 RID: 689
	public class UIUserRolesController : UIGameObject
	{
		// Token: 0x06000B99 RID: 2969 RVA: 0x000368C5 File Offset: 0x00034AC5
		private void Start()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "Start", Array.Empty<object>());
			}
			this.addUserRoleButton.onClick.AddListener(new UnityAction(this.AddUserRole));
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x000368FC File Offset: 0x00034AFC
		private void AddUserRole()
		{
			if (this.verboseLogging)
			{
				DebugUtility.LogMethod(this, "AddUserRole", Array.Empty<object>());
			}
			List<User> list = ((this.addUsersFromThisModel == null) ? null : this.addUsersFromThisModel.Users.ToList<User>());
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.InitializeAddUserFlow(this.model, list);
		}

		// Token: 0x040009C9 RID: 2505
		[SerializeField]
		private UIUserRolesModel model;

		// Token: 0x040009CA RID: 2506
		[SerializeField]
		private UIButton addUserRoleButton;

		// Token: 0x040009CB RID: 2507
		[SerializeField]
		private UIUserRolesModel addUsersFromThisModel;

		// Token: 0x040009CC RID: 2508
		[Header("Debugging")]
		[SerializeField]
		private bool verboseLogging;
	}
}
