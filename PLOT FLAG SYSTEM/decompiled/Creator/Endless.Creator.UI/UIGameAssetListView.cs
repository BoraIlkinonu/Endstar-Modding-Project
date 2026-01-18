using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameAssetListView : UIBaseRoleInteractableListView<UIGameAsset>
{
	public enum SelectActions
	{
		ListSelect,
		StaticSelect,
		ViewDetails
	}

	[field: Header("UIGameAssetListView")]
	[field: SerializeField]
	public SelectActions SelectAction { get; private set; } = SelectActions.ViewDetails;

	[field: SerializeField]
	public bool ViewInLibraryMarker { get; private set; }
}
