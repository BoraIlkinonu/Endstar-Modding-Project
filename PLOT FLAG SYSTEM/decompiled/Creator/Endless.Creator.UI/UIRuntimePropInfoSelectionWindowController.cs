using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIRuntimePropInfoSelectionWindowController : UIWindowController
{
	[SerializeField]
	private UIRuntimePropInfoSelectionWindowView view;

	[SerializeField]
	private UIRuntimePropInfoListModel runtimePropInfoListModel;

	[SerializeField]
	private UIButton confirmButton;

	protected override void Start()
	{
		base.Start();
		confirmButton.onClick.AddListener(Confirm);
	}

	private void Confirm()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.LogMethod(this, "Confirm");
		}
		view.OnConfirm?.Invoke(runtimePropInfoListModel.SelectedTypedList);
		Close();
	}
}
