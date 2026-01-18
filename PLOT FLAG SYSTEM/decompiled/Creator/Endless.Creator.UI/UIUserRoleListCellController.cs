using System;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIUserRoleListCellController : UIBaseListCellController<UserRole>
{
	[Header("UIUserRoleListCellController")]
	[SerializeField]
	private UIButton changeRoleButton;

	protected override void Start()
	{
		base.Start();
		changeRoleButton.onClick.AddListener(ChangeRole);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}

	private void ChangeRole()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "ChangeRole");
		}
		UIUserRoleListModel uIUserRoleListModel = (UIUserRoleListModel)base.ListModel;
		UIUserRoleListCellView uIUserRoleListCellView = (UIUserRoleListCellView)View;
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.InitializeChangeUserFlow(uIUserRoleListModel.UserRolesModel, base.Model, uIUserRoleListCellView.UserName);
	}
}
