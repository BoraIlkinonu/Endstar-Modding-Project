using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public abstract class UIItemSelectionToolPanelController<T, TItemType> : UIBaseToolPanelController<T> where T : EndlessTool
{
	[Header("UIItemSelectionToolPanelController")]
	[SerializeField]
	private UIButton deselectButton;

	[SerializeField]
	private UIButton infoButton;

	protected UIItemSelectionToolPanelView<T, TItemType> ItemSelectionToolPanelView { get; private set; }

	protected override void Start()
	{
		base.Start();
		ItemSelectionToolPanelView = (UIItemSelectionToolPanelView<T, TItemType>)View;
		deselectButton.onClick.AddListener(Deselect);
		infoButton.onClick.AddListener(ViewInfo);
	}

	public abstract void Deselect();

	private void ViewInfo()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("ViewInfo", this);
		}
		ItemSelectionToolPanelView.Undock();
	}
}
