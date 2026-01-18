using Endless.Data;
using Endless.Matchmaking;
using Endless.Shared;
using Endless.Shared.UI;
using Runtime.Shared.Matchmaking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Endless.Creator.UI;

public class UIAdminWindowDisplayHandler : UIGameObject
{
	[Header("Debugging")]
	[SerializeField]
	private bool verboseLogging;

	private PlayerInputActions playerInputActions;

	private bool isAdmin;

	private void OnEnable()
	{
		if (verboseLogging)
		{
			Debug.Log("OnEnable", this);
		}
		if (playerInputActions == null)
		{
			playerInputActions = new PlayerInputActions();
		}
		if (MatchmakingClientController.Instance.LocalMatch != null)
		{
			MatchData matchData = MatchmakingClientController.Instance.LocalMatch.GetMatchData();
			isAdmin = matchData.IsAdminSession();
			playerInputActions.Player.DisplayGameLibraryModerationWindow.performed += DisplayGameLibraryModerationWindow;
			playerInputActions.Player.DisplayGameLibraryModerationWindow.Enable();
		}
	}

	private void OnDisable()
	{
		if (verboseLogging)
		{
			Debug.Log("OnDisable", this);
		}
		isAdmin = false;
		playerInputActions.Player.DisplayGameLibraryModerationWindow.performed -= DisplayGameLibraryModerationWindow;
		playerInputActions.Player.DisplayGameLibraryModerationWindow.Disable();
	}

	private void DisplayGameLibraryModerationWindow(InputAction.CallbackContext callbackContext)
	{
		if (verboseLogging)
		{
			Debug.Log(string.Format("{0} ( {1} : {2} )", "DisplayGameLibraryModerationWindow", "callbackContext", callbackContext), this);
		}
		if (isAdmin)
		{
			DisplayGameLibraryModerationWindow();
		}
	}

	private void DisplayGameLibraryModerationWindow()
	{
		if (verboseLogging)
		{
			Debug.Log("DisplayGameLibraryModerationWindow", this);
		}
		if (!MonoBehaviourSingleton<UIWindowManager>.Instance.IsDisplaying || !(MonoBehaviourSingleton<UIWindowManager>.Instance.Displayed.GetType() == typeof(UIGameLibraryModerationWindowView)))
		{
			UIGameLibraryModerationWindowView.Display();
		}
	}
}
