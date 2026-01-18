using Endless.Shared.Debugging;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Endless.Gameplay.UI;

public class UIOnScreenButtonHandler : OnScreenControl
{
	[InputControl(layout = "Button")]
	[SerializeField]
	private string controlPathValue;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	protected override string controlPathInternal
	{
		get
		{
			return controlPathValue;
		}
		set
		{
			controlPathValue = value;
		}
	}

	public void SetButtonState(bool down)
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "SetButtonState", down);
		}
		SendValueToControl(down ? 1f : 0f);
	}
}
