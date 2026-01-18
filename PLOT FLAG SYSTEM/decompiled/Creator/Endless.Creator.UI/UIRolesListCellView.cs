using Endless.Shared;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRolesListCellView : UIBaseListCellView<Roles>
{
	[Header("UIRolesListCellView")]
	[SerializeField]
	private TextMeshProUGUI roleText;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private UIRolesDescriptionsDictionary rolesDescriptionsDictionary;

	[SerializeField]
	private GameObject permissionDeniedCover;

	public override void View(UIBaseListView<Roles> listView, int dataIndex)
	{
		base.View(listView, dataIndex);
		roleText.text = base.Model.ToString();
		descriptionText.text = rolesDescriptionsDictionary[base.Model].Game;
		UIRolesListModel uIRolesListModel = (UIRolesListModel)base.ListModel;
		bool flag = uIRolesListModel.LocalClientRole == Roles.Owner || uIRolesListModel.LocalClientRole.IsGreaterThanOrEqualTo(base.Model);
		permissionDeniedCover.gameObject.SetActive(!flag);
	}
}
