using System;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibraryFilterListCellController : UIBaseListCellController<UIGameAssetTypes>
{
	[Header("UIGameLibraryFilterListCellController")]
	[SerializeField]
	private UIButton button;

	protected override void Start()
	{
		base.Start();
		button.onClick.AddListener(ToggleSelected);
	}

	protected override void OnAddButton()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnAddButton");
		}
		throw new NotImplementedException();
	}
}
