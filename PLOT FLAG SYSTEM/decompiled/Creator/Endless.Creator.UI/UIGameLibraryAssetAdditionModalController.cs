using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryAssetAdditionModalController : UIGameObject
{
	[SerializeField]
	private UIGameLibraryAssetAdditionModalView view;

	[SerializeField]
	private UIDropdownEnum gameAssetSourceDropdown;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		gameAssetSourceDropdown.OnEnumValueChanged.AddListener(view.ViewSource);
	}
}
