using Endless.Gameplay;
using Endless.Shared;
using Endless.Shared.Debugging;
using Endless.Shared.UI;
using UnityEngine;

namespace Endless.Creator.UI;

public class UIGhostModeController : UIGameObject
{
	[SerializeField]
	private UIButton ghostModeToggleButton;

	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private void Start()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "Start");
		}
		ghostModeToggleButton.onClick.AddListener(ToggleGhostMode);
	}

	private void ToggleGhostMode()
	{
		if (verboseLogging)
		{
			DebugUtility.LogMethod(this, "ToggleGhostMode");
		}
		MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayerObject().PlayerNetworkController.ToggleGhostMode();
	}
}
