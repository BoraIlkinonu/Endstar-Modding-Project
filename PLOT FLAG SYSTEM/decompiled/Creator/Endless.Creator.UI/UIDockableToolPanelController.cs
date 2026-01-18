using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIDockableToolPanelController : UIGameObject
{
	[SerializeField]
	private InterfaceReference<IDockableToolPanelView> dockableToolPanelView;

	[SerializeField]
	private UIButton dockButton;

	[SerializeField]
	private UIButton undockButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		dockButton.onClick.AddListener(dockableToolPanelView.Interface.Dock);
		undockButton.onClick.AddListener(dockableToolPanelView.Interface.Undock);
	}
}
