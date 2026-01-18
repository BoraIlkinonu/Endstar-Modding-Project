using System.Collections.Generic;
using System.Linq;
using Endless.Shared;
using Endless.Shared.DataTypes;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIUserRolesController : UIGameObject
{
	[SerializeField]
	private UIUserRolesModel model;

	[SerializeField]
	private UIButton addUserRoleButton;

	[SerializeField]
	private UIUserRolesModel addUsersFromThisModel;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		addUserRoleButton.onClick.AddListener(AddUserRole);
	}

	private void AddUserRole()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "AddUserRole");
		}
		List<User> usersToDisplay = ((addUsersFromThisModel == null) ? null : addUsersFromThisModel.Users.ToList());
		MonoBehaviourSingleton<UIUserRoleWizard>.Instance.InitializeAddUserFlow(model, usersToDisplay);
	}
}
