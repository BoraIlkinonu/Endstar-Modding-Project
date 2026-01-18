using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Endless.Creator.UI;

public abstract class UIDockableToolPanelView<T> : UIBaseToolPanelView<T>, IDockableToolPanelView where T : EndlessTool
{
	[Header("UIDockableToolPanelView")]
	[SerializeField]
	private Image toolIconImage;

	[SerializeField]
	private UIToolTypeColorDictionary toolTypeColorDictionary;

	[SerializeField]
	private UIDisplayAndHideHandler dockingDisplayAndHideHandler;

	[SerializeField]
	private UIDisplayAndHideHandler undockButtonDisplayAndHideHandler;

	private bool isDocked;

	protected override void Start()
	{
		base.Start();
		undockButtonDisplayAndHideHandler.SetToHideEnd(triggerUnityEvent: true);
		MonoBehaviourSingleton<ToolManager>.Instance.OnSetActiveToolToSameTool.AddListener(ToggleDockIfActivatedSameTool);
		toolIconImage.sprite = Tool.Icon;
		toolIconImage.color = toolTypeColorDictionary[Tool.ToolType];
	}

	public override void Display()
	{
		base.Display();
		Undock();
	}

	public override void Hide()
	{
		base.Hide();
		dockingDisplayAndHideHandler.Hide();
		undockButtonDisplayAndHideHandler.Hide();
		isDocked = false;
	}

	public void Dock()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Dock", this);
		}
		dockingDisplayAndHideHandler.Hide();
		undockButtonDisplayAndHideHandler.Display();
		isDocked = true;
	}

	public void Undock()
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("Undock", this);
		}
		dockingDisplayAndHideHandler.Display();
		undockButtonDisplayAndHideHandler.Hide();
		isDocked = false;
	}

	private void ToggleDockIfActivatedSameTool(EndlessTool activeTool)
	{
		if (base.VerboseLogging)
		{
			DebugUtility.Log("ToggleDockIfActivatedSameTool ( activeTool: " + ((activeTool != null) ? activeTool.GetType().Name : "null") + " )", this);
		}
		if (!(activeTool == null) && !(activeTool.GetType() != typeof(T)))
		{
			if (isDocked)
			{
				Undock();
			}
			else
			{
				Dock();
			}
		}
	}
}
