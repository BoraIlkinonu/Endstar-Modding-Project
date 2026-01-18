using Endless.Gameplay;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UILevelDestinationSelectionListView : UIBaseListView<LevelDestination>
{
	public enum SelectionTypes
	{
		ApplyToProperty,
		LocalListToggleSelected
	}

	[field: Header("UILevelDestinationSelectionListView")]
	[field: SerializeField]
	public SelectionTypes SelectionType { get; private set; }
}
