using Endless.Creator.LevelEditing.Runtime;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIEraseToolPanelController : UIGameObject
{
	[SerializeField]
	private UIToggle terrain;

	[SerializeField]
	private UIToggle unwiredProps;

	[SerializeField]
	private UIToggle wiredProps;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private EraseTool eraseTool;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		eraseTool = MonoBehaviourSingleton<ToolManager>.Instance.RequestToolInstance<EraseTool>();
		terrain.OnChange.AddListener(ToggleTerrain);
		unwiredProps.OnChange.AddListener(ToggleUnwiredProps);
		wiredProps.OnChange.AddListener(ToggleWiredProps);
	}

	private void ToggleTerrain(bool state)
	{
		if (verboseLogging)
		{
			Debug.LogFormat(this, "ToggleTerrain ( state: {0} )", state);
		}
		eraseTool.ToggleCurrentFunction(EraseToolFunction.Terrain, state);
	}

	private void ToggleUnwiredProps(bool state)
	{
		if (verboseLogging)
		{
			Debug.LogFormat(this, "ToggleUnwiredProps ( state: {0} )", state);
		}
		eraseTool.ToggleCurrentFunction(EraseToolFunction.UnwiredProps, state);
	}

	private void ToggleWiredProps(bool state)
	{
		if (verboseLogging)
		{
			Debug.LogFormat(this, "ToggleWiredProps ( state: {0} )", state);
		}
		eraseTool.ToggleCurrentFunction(EraseToolFunction.WiredProps, state);
		if (!state)
		{
			eraseTool.ToggleCurrentFunction(EraseToolFunction.UnwiredProps, state: true);
		}
	}
}
