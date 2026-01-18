using System.Collections.Generic;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGameLibrarySelectionWindowController : UIWindowController
{
	[Header("UIGameLibrarySelectionWindowController")]
	[SerializeField]
	private UIGameLibraryListModel gameLibraryListModel;

	[SerializeField]
	private UIButton confirmButton;

	private UIGameLibrarySelectionWindowView view;

	protected override void Start()
	{
		base.Start();
		confirmButton.onClick.AddListener(OnConfirm);
		view = BaseWindowView as UIGameLibrarySelectionWindowView;
	}

	private void OnConfirm()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "OnConfirm");
		}
		IReadOnlyList<UIGameAsset> selectedTypedList = gameLibraryListModel.SelectedTypedList;
		if (selectedTypedList.Count > 0)
		{
			view.OnSelected(selectedTypedList[0].AssetID);
		}
		Close();
	}
}
