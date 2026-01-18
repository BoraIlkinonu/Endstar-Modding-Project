using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIBaseToolPanelController<T> : UIGameObject where T : EndlessTool
{
	[Header("UIBaseToolPanelController")]
	[SerializeField]
	protected UIBaseToolPanelView<T> View;

	protected T Tool;

	protected bool VerboseLogging { get; set; }

	protected virtual void Start()
	{
		if (VerboseLogging)
		{
			DebugUtility.Log("Start", this);
		}
		Tool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<T>();
	}
}
