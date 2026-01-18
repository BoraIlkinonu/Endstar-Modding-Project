using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using TMPro;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRolesListView : UIBaseListView<Roles>
{
	[Header("UIRolesListView")]
	[SerializeField]
	private UIRolesDescriptionsDictionary rolesDescriptionsDictionary;

	[SerializeField]
	private TextMeshProUGUI dynamicTextSizeReference;

	public override float GetCellViewSize(int index)
	{
		if (base.SuperVerboseLogging)
		{
			DebugUtility.LogMethod(this, "GetCellViewSize", index);
		}
		Roles key = base.Model[index];
		string game = rolesDescriptionsDictionary[key].Game;
		dynamicTextSizeReference.text = game;
		dynamicTextSizeReference.ForceMeshUpdate();
		return dynamicTextSizeReference.GetRenderedValues().y + 83f;
	}
}
