using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.Events;

namespace Endless.Creator.UI
{
	// Token: 0x02000191 RID: 401
	public class UIUserRoleListCellController : UIBaseListCellController<UserRole>
	{
		// Token: 0x060005D6 RID: 1494 RVA: 0x0001E0B5 File Offset: 0x0001C2B5
		protected override void Start()
		{
			base.Start();
			this.changeRoleButton.onClick.AddListener(new UnityAction(this.ChangeRole));
		}

		// Token: 0x060005D7 RID: 1495 RVA: 0x0001E0D9 File Offset: 0x0001C2D9
		protected override void OnAddButton()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "OnAddButton", Array.Empty<object>());
			}
			throw new NotImplementedException();
		}

		// Token: 0x060005D8 RID: 1496 RVA: 0x0001E0F8 File Offset: 0x0001C2F8
		private void ChangeRole()
		{
			if (base.VerboseLogging)
			{
				DebugUtility.LogMethod(this, "ChangeRole", Array.Empty<object>());
			}
			UIUserRoleListModel uiuserRoleListModel = (UIUserRoleListModel)base.ListModel;
			UIUserRoleListCellView uiuserRoleListCellView = (UIUserRoleListCellView)this.View;
			MonoBehaviourSingleton<UIUserRoleWizard>.Instance.InitializeChangeUserFlow(uiuserRoleListModel.UserRolesModel, base.Model, uiuserRoleListCellView.UserName);
		}

		// Token: 0x04000516 RID: 1302
		[Header("UIUserRoleListCellController")]
		[SerializeField]
		private UIButton changeRoleButton;
	}
}
